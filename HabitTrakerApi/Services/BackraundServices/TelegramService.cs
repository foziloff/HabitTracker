using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using System.Collections.Concurrent;
using HabitTrakerApi.Models.Enums;
using HabitTrakerApi.DbContext;
using HabitTrakerApi.Common;
using Microsoft.EntityFrameworkCore;
using HabitTrakerApi.Models.Data;

namespace HabitTrakerApi.Services.BackraundServices;

public class TelegramService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TelegramBotClient _telegramBotClient;
    
    // Состояния авторизации[cite: 12]
    private readonly ConcurrentDictionary<long, TelegramAuthState> _authStates = new();
    private readonly ConcurrentDictionary<long, string> _currentUserLogin = new();

    // Внутренние состояния бота для создания сущностей
    private enum BotActionState
    {
        None,
        WaitingForCategoryName,
        WaitingForHabitTitle
    }
    
    private readonly ConcurrentDictionary<long, BotActionState> _actionStates = new();
    
    // Временное хранилище для черновика привычки (до того как сохраним в БД)
    private readonly ConcurrentDictionary<long, Habit> _draftHabits = new();

    public TelegramService(IServiceScopeFactory scope, IConfiguration config)
    {
        _telegramBotClient = new TelegramBotClient(config["TelegramBot:Token"]!); 
        _scopeFactory = scope;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [] 
        };

        _telegramBotClient.StartReceiving(
            updateHandler: Update,
            errorHandler: Error,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task Update(ITelegramBotClient client, Update update, CancellationToken token)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Обработка Inline-кнопок
        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
        {
            await HandleCallbackQuery(client, update.CallbackQuery, context, token);
            return;
        }

        // Обработка текстовых сообщений
        var message = update.Message;
        if (message == null || string.IsNullOrEmpty(message.Text)) return;
        
        long chatId = message.Chat.Id;

        // Идентифицируем пользователя в БД[cite: 6]
        var dbUser = await context.Users.FirstOrDefaultAsync(u => u.ChatId == chatId, token);
        if (dbUser != null)
        {
            _authStates[chatId] = TelegramAuthState.Authorized; //[cite: 12]
        }

        // --- 1. ЛОГИКА АВТОРИЗОВАННОГО ПОЛЬЗОВАТЕЛЯ ---
        if (_authStates.TryGetValue(chatId, out var authState) && authState == TelegramAuthState.Authorized)
        {
            var actionState = _actionStates.GetValueOrDefault(chatId, BotActionState.None);

            // Обработка команд главного меню
            switch (message.Text)
            {
                case "📅 Мои привычки":
                    _actionStates[chatId] = BotActionState.None;
                    await ShowUserHabits(client, chatId, dbUser!.Id, context, token);
                    return;
                case "📊 Статистика":
                    _actionStates[chatId] = BotActionState.None;
                    await ShowUserStats(client, chatId, dbUser!.Id, context, token);
                    return;
                case "📁 Добавить категорию":
                    _actionStates[chatId] = BotActionState.WaitingForCategoryName;
                    await client.SendMessage(chatId, "Введите название новой категории:", cancellationToken: token);
                    return;
                case "➕ Добавить привычку":
                    _actionStates[chatId] = BotActionState.None;
                    await StartHabitCreationFlow(client, chatId, context, token);
                    return;
            }

            // Обработка ввода данных для создания категории[cite: 1]
            if (actionState == BotActionState.WaitingForCategoryName)
            {
                var category = new Category { Name = message.Text };
                context.Categories.Add(category);
                await context.SaveChangesAsync(token);

                _actionStates[chatId] = BotActionState.None;
                await client.SendMessage(chatId, $"✅ Категория '{category.Name}' успешно добавлена!", replyMarkup: GetMainMenu(), cancellationToken: token);
                return;
            }

            // Обработка ввода названия для новой привычки[cite: 3]
            if (actionState == BotActionState.WaitingForHabitTitle)
            {
                if (_draftHabits.TryGetValue(chatId, out var draftHabit))
                {
                    draftHabit.UserId = dbUser!.Id;
                    draftHabit.Title = message.Text;
                    draftHabit.Status = HabitStatus.Active; //[cite: 9]
                    draftHabit.CreatedAt = DateTime.UtcNow;

                    context.Habits.Add(draftHabit);
                    await context.SaveChangesAsync(token);

                    _actionStates[chatId] = BotActionState.None;
                    _draftHabits.TryRemove(chatId, out _);

                    await client.SendMessage(chatId, $"✅ Привычка '{draftHabit.Title}' успешно создана!", replyMarkup: GetMainMenu(), cancellationToken: token);
                }
                return;
            }
        }

        // --- 2. ЛОГИКА АВТОРИЗАЦИИ (Если пользователь не авторизован) ---
        if (message.Text.Equals("/start", StringComparison.OrdinalIgnoreCase))
        {
            if (dbUser is not null)
            {
                await client.SendMessage(chatId, "Привет! Вы уже авторизованы.", replyMarkup: GetMainMenu(), cancellationToken: token);
                return;
            }

            _authStates[chatId] = TelegramAuthState.None;
            var replyKeyboards = new ReplyKeyboardMarkup(new[] { new KeyboardButton[] { "/auth" } }) { ResizeKeyboard = true };
            await client.SendMessage(chatId, "Привет! Пожалуйста, авторизуйтесь.", replyMarkup: replyKeyboards, cancellationToken: token);
            return;
        }

        if (message.Text.ToLower() == "/auth")
        {
            if (dbUser is not null)
            {
                await client.SendMessage(chatId, "Вы уже авторизованы.", replyMarkup: GetMainMenu(), cancellationToken: token);
                return;
            }
            _authStates[chatId] = TelegramAuthState.WaitingLogin;
            await client.SendMessage(chatId, "Введите логин:", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);
            return;
        }

        if (_authStates.TryGetValue(chatId, out var loginState) && loginState == TelegramAuthState.WaitingLogin)
        {
            var userToAuth = await context.Users.FirstOrDefaultAsync(u => u.Login == message.Text, token);
            if (userToAuth is null)
            {
                _authStates[chatId] = TelegramAuthState.None;
                await client.SendMessage(chatId, "Неверный логин! Нажмите /auth для повтора.", cancellationToken: token);
                return;
            }
            
            _authStates[chatId] = TelegramAuthState.WaitingPassword;
            _currentUserLogin[chatId] = userToAuth.Login;
            await client.SendMessage(chatId, "Введите свой пароль:", cancellationToken: token);
            return;
        }

        if (_authStates.TryGetValue(chatId, out var passState) && passState == TelegramAuthState.WaitingPassword)
        {
            var userToAuth = await context.Users.AsTracking().FirstOrDefaultAsync(u => u.Login == _currentUserLogin[chatId], token);

            if (userToAuth != null && PasswordHasher.Verify(message.Text.Trim(), userToAuth.Password))
            {
                userToAuth.ChatId = chatId;
                _authStates[chatId] = TelegramAuthState.Authorized;
                await context.SaveChangesAsync(token);
                
                await client.SendMessage(chatId, "Вы успешно авторизовались!", replyMarkup: GetMainMenu(), cancellationToken: token);
            }
            else
            {
                _authStates[chatId] = TelegramAuthState.None;
                await client.SendMessage(chatId, "Неверный пароль! Нажмите /auth чтобы попробовать снова.", cancellationToken: token);
            }
        }
    }

    // --- ОБРАБОТЧИКИ СОБЫТИЙ ---

    private async Task HandleCallbackQuery(ITelegramBotClient client, CallbackQuery callbackQuery, AppDbContext context, CancellationToken token)
    {
        var data = callbackQuery.Data;
        long chatId = callbackQuery.Message!.Chat.Id;
        if (data == null) return;

        // 1. Выбор категории при создании привычки
        if (data.StartsWith("cat_"))
        {
            int categoryId = int.Parse(data.Replace("cat_", ""));
            _draftHabits[chatId] = new Habit { CategoryId = categoryId }; //[cite: 3]
            
            // Запрашиваем тип привычки[cite: 10]
            var typeKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Ежедневная", $"type_{(int)HabitType.Daily}") },
                new[] { InlineKeyboardButton.WithCallbackData("Еженедельная", $"type_{(int)HabitType.Weekly}") },
                new[] { InlineKeyboardButton.WithCallbackData("Ежемесячная", $"type_{(int)HabitType.Monthly}") },
                new[] { InlineKeyboardButton.WithCallbackData("Одноразовая", $"type_{(int)HabitType.Disposable}") }
            });

            await client.EditMessageText(chatId, callbackQuery.Message.MessageId, "Выберите тип привычки:", replyMarkup: typeKeyboard, cancellationToken: token);
            return;
        }

        // 2. Выбор типа привычки
        if (data.StartsWith("type_"))
        {
            int typeId = int.Parse(data.Replace("type_", ""));
            if (_draftHabits.TryGetValue(chatId, out var draft))
            {
                draft.Type = (HabitType)typeId; //[cite: 10]
                _actionStates[chatId] = BotActionState.WaitingForHabitTitle;

                await client.EditMessageText(chatId, callbackQuery.Message.MessageId, "Отлично! Теперь отправьте текстовым сообщением название привычки:", cancellationToken: token);
            }
            return;
        }

        // 3. Отметка выполнения привычки
        if (data.StartsWith("done_"))
        {
            int habitId = int.Parse(data.Replace("done_", ""));
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            
            bool alreadyLogged = await context.HabitLogs.AnyAsync(l => l.HabitId == habitId && l.DoneDate == today, token); //[cite: 4]

            if (!alreadyLogged)
            {
                var log = new HabitLog //[cite: 4]
                {
                    HabitId = habitId,
                    DoneDate = today,
                    Value = 1,
                    Note = "Telegram"
                };
                context.HabitLogs.Add(log);
                await context.SaveChangesAsync(token);

                // Подсчет режима (стрика)
                var logs = await context.HabitLogs.Where(l => l.HabitId == habitId).Select(l => l.DoneDate).OrderByDescending(d => d).ToListAsync(token);
                int streak = CalculateStreak(logs);

                await client.AnswerCallbackQuery(callbackQuery.Id, $"Отмечено! 🔥 Серия: {streak} выполнений подряд!", showAlert: true, cancellationToken: token);
                await client.EditMessageReplyMarkup(chatId, callbackQuery.Message.MessageId, replyMarkup: null, cancellationToken: token);
            }
            else
            {
                await client.AnswerCallbackQuery(callbackQuery.Id, "Вы уже отмечали это сегодня!", cancellationToken: token);
            }
        }
    }

    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---

    private ReplyKeyboardMarkup GetMainMenu()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "📅 Мои привычки", "📊 Статистика" },
            new KeyboardButton[] { "➕ Добавить привычку", "📁 Добавить категорию" }
        })
        { ResizeKeyboard = true };
    }

    private async Task StartHabitCreationFlow(ITelegramBotClient client, long chatId, AppDbContext context, CancellationToken token)
    {
        var categories = await context.Categories.ToListAsync(token); //[cite: 1]
        if (!categories.Any())
        {
            await client.SendMessage(chatId, "Сначала создайте хотя бы одну категорию (кнопка '📁 Добавить категорию').", cancellationToken: token);
            return;
        }

        var buttons = categories.Select(c => InlineKeyboardButton.WithCallbackData(c.Name, $"cat_{c.Id}")).Chunk(2).ToList();
        await client.SendMessage(chatId, "Выберите категорию для новой привычки:", replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: token);
    }

    private async Task ShowUserHabits(ITelegramBotClient client, long chatId, int userId, AppDbContext context, CancellationToken token)
    {
        var habits = await context.Habits.Include(h => h.Logs).Where(h => h.UserId == userId && h.Status == HabitStatus.Active).ToListAsync(token); //[cite: 3],[cite: 9]

        if (!habits.Any())
        {
            await client.SendMessage(chatId, "У вас пока нет активных привычек.", cancellationToken: token);
            return;
        }

        await client.SendMessage(chatId, "Ваши активные привычки:", cancellationToken: token);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        foreach (var habit in habits)
        {
            bool isDone = CheckIfDone(habit, today);
            
            string statusEmoji = isDone ? "✅" : "⏳";
            string typeEmoji = habit.Type switch //[cite: 10]
            {
                HabitType.Daily => "Ежедневно",
                HabitType.Weekly => "Еженедельно",
                HabitType.Monthly => "Ежемесячно",
                HabitType.Disposable => "Единоразово",
                _ => ""
            };

            string text = $"{statusEmoji} <b>{habit.Title}</b> ({typeEmoji})";
            InlineKeyboardMarkup? inlineKeyboard = null;

            if (!isDone)
            {
                inlineKeyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("✔ Выполнил!", $"done_{habit.Id}"));
            }

            await client.SendMessage(chatId, text, parseMode: ParseMode.Html, replyMarkup: inlineKeyboard, cancellationToken: token);
        }
    }

    private async Task ShowUserStats(ITelegramBotClient client, long chatId, int userId, AppDbContext context, CancellationToken token)
    {
        var habits = await context.Habits.Include(h => h.Logs).Where(h => h.UserId == userId).ToListAsync(token); //[cite: 3]
        
        int total = habits.Count;
        int dailyCount = habits.Count(h => h.Type == HabitType.Daily); //[cite: 10]
        int weeklyCount = habits.Count(h => h.Type == HabitType.Weekly); //[cite: 10]
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int doneToday = habits.Count(h => h.Type == HabitType.Daily && h.Logs.Any(l => l.DoneDate == today));

        string stats = $"📊 <b>Ваша статистика:</b>\n\n" +
                       $"Всего привычек: {total}\n" +
                       $"Ежедневных: {dailyCount} (выполнено сегодня: {doneToday})\n" +
                       $"Еженедельных: {weeklyCount}";

        await client.SendMessage(chatId, stats, parseMode: ParseMode.Html, cancellationToken: token);
    }

    // Проверка, выполнена ли привычка, с учетом её типа (HabitType)[cite: 10]
    private bool CheckIfDone(Habit habit, DateOnly today)
    {
        if (!habit.Logs.Any()) return false;

        return habit.Type switch //[cite: 10]
        {
            HabitType.Daily => habit.Logs.Any(l => l.DoneDate == today),
            HabitType.Weekly => habit.Logs.Any(l => l.DoneDate >= today.AddDays(-7)), // Упрощенная неделя (последние 7 дней)
            HabitType.Monthly => habit.Logs.Any(l => l.DoneDate.Month == today.Month && l.DoneDate.Year == today.Year),
            HabitType.Disposable => true, // Если есть лог, то одноразовая выполнена навсегда
            _ => false
        };
    }

    // Подсчет режима (стрика) для ежедневных привычек
    private int CalculateStreak(List<DateOnly> doneDatesDesc)
    {
        if (!doneDatesDesc.Any()) return 0;
        int streak = 0;
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        if (doneDatesDesc.First() != currentDate && doneDatesDesc.First() != currentDate.AddDays(-1)) return 0;

        DateOnly expectedDate = doneDatesDesc.First();
        foreach(var date in doneDatesDesc)
        {
            if (date == expectedDate)
            {
                streak++;
                expectedDate = expectedDate.AddDays(-1);
            }
            else break;
        }
        return streak;
    }

    public async Task SendMessageAsync(long chatId, string message, CancellationToken token = default)
    {
        await _telegramBotClient.SendMessage(chatId, message, cancellationToken: token); 
    }

    private Task Error(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
    {
        Console.WriteLine($"Telegram error: {exception.Message}");
        return Task.CompletedTask;
    }
}