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
    
    private readonly ConcurrentDictionary<long, TelegramAuthState> _authStates = new();
    private readonly ConcurrentDictionary<long, string> _currentUserLogin = new();

    private enum BotActionState
    {
        None,
        WaitingForCategoryName,
        WaitingForHabitTitle,
        WaitingForHabitDayOfMonth,
        WaitingForReminderTime,
        WaitingForProof
    }
    
    private readonly ConcurrentDictionary<long, BotActionState> _actionStates = new();
    private readonly ConcurrentDictionary<long, Habit> _draftHabits = new();
    private readonly ConcurrentDictionary<long, int> _pendingProofHabitIds = new();

    public TelegramService(IServiceScopeFactory scope, IConfiguration config)
    {
        _telegramBotClient = new TelegramBotClient(config["TelegramBot:Token"]!); 
        _scopeFactory = scope;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions { AllowedUpdates = [] };

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

        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
        {
            await HandleCallbackQuery(client, update.CallbackQuery, context, token);
            return;
        }

        var message = update.Message;
        if (message == null) return;
        
        long chatId = message.Chat.Id;

        var dbUser = await context.Users.FirstOrDefaultAsync(u => u.ChatId == chatId, token);
        if (dbUser != null)
        {
            _authStates[chatId] = TelegramAuthState.Authorized;
        }

        if (_authStates.TryGetValue(chatId, out var authState) && authState == TelegramAuthState.Authorized)
        {
            var actionState = _actionStates.GetValueOrDefault(chatId, BotActionState.None);

            if (actionState == BotActionState.WaitingForProof && (message.Photo != null || message.Video != null))
            {
                if (_pendingProofHabitIds.TryGetValue(chatId, out int habitId))
                {
                    await client.SendMessage(chatId, "Всё, верю! Молодец! 👏", cancellationToken: token);
                    await SaveHabitLog(client, chatId, habitId, context, "С подтверждением (медиа)", token);
                    
                    _actionStates[chatId] = BotActionState.None;
                    _pendingProofHabitIds.TryRemove(chatId, out _);
                }
                return;
            }

            if (string.IsNullOrEmpty(message.Text)) return;

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

            if (actionState == BotActionState.WaitingForCategoryName)
            {
                var category = new Category { Name = message.Text };
                context.Categories.Add(category);
                await context.SaveChangesAsync(token);

                _actionStates[chatId] = BotActionState.None;
                await client.SendMessage(chatId, $"✅ Категория '{category.Name}' успешно добавлена!", replyMarkup: GetMainMenu(), cancellationToken: token);
                return;
            }

            if (actionState == BotActionState.WaitingForHabitTitle)
            {
                if (_draftHabits.TryGetValue(chatId, out var draftHabit))
                {
                    draftHabit.Title = message.Text;

                    if (draftHabit.Type == HabitType.Weekly)
                    {
                        var daysKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new[] { InlineKeyboardButton.WithCallbackData("Понедельник", "dow_1"), InlineKeyboardButton.WithCallbackData("Вторник", "dow_2") },
                            new[] { InlineKeyboardButton.WithCallbackData("Среда", "dow_3"), InlineKeyboardButton.WithCallbackData("Четверг", "dow_4") },
                            new[] { InlineKeyboardButton.WithCallbackData("Пятница", "dow_5"), InlineKeyboardButton.WithCallbackData("Суббота", "dow_6") },
                            new[] { InlineKeyboardButton.WithCallbackData("Воскресенье", "dow_0") }
                        });
                        _actionStates[chatId] = BotActionState.None; 
                        await client.SendMessage(chatId, "Выберите день недели для выполнения:", replyMarkup: daysKeyboard, cancellationToken: token);
                    }
                    else if (draftHabit.Type == HabitType.Monthly)
                    {
                        _actionStates[chatId] = BotActionState.WaitingForHabitDayOfMonth;
                        await client.SendMessage(chatId, "Введите число месяца, когда нужно выполнять привычку (от 1 до 31):", cancellationToken: token);
                    }
                    else 
                    {
                        _actionStates[chatId] = BotActionState.WaitingForReminderTime;
                        await client.SendMessage(chatId, "Введите время уведомления в формате ЧЧ:ММ (например, 08:30):", cancellationToken: token);
                    }
                }
                return;
            }

            if (actionState == BotActionState.WaitingForHabitDayOfMonth)
            {
                if (int.TryParse(message.Text, out int day) && day >= 1 && day <= 31)
                {
                    if (_draftHabits.TryGetValue(chatId, out var draftHabit))
                    {
                        draftHabit.ExecutionDayOfMonth = day;
                        _actionStates[chatId] = BotActionState.WaitingForReminderTime;
                        await client.SendMessage(chatId, "Отлично! Теперь введите время уведомления (например, 09:00):", cancellationToken: token);
                    }
                }
                else
                {
                    await client.SendMessage(chatId, "Пожалуйста, введите корректное число от 1 до 31.", cancellationToken: token);
                }
                return;
            }

            if (actionState == BotActionState.WaitingForReminderTime)
            {
                if (TimeOnly.TryParse(message.Text, out TimeOnly time))
                {
                    if (_draftHabits.TryGetValue(chatId, out var draftHabit))
                    {
                        draftHabit.UserId = dbUser!.Id;
                        draftHabit.Status = HabitStatus.Active;
                        draftHabit.CreatedAt = DateTime.UtcNow;

                        context.Habits.Add(draftHabit);
                        
                        var reminder = new Reminder
                        {
                            Habit = draftHabit,
                            ReminderTime = time,
                            IsEnabled = true
                        };
                        context.Reminders.Add(reminder);

                        await context.SaveChangesAsync(token);

                        _actionStates[chatId] = BotActionState.None;
                        _draftHabits.TryRemove(chatId, out _);

                        await client.SendMessage(chatId, $"✅ Привычка '{draftHabit.Title}' успешно создана! Напоминание установлено на {time:HH:mm}.", replyMarkup: GetMainMenu(), cancellationToken: token);
                    }
                }
                else
                {
                    await client.SendMessage(chatId, "Неверный формат времени. Пожалуйста, введите время в формате ЧЧ:ММ (например, 08:30 или 21:00):", cancellationToken: token);
                }
                return;
            }
        }

        if (string.IsNullOrEmpty(message?.Text)) return;

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

    private async Task HandleCallbackQuery(ITelegramBotClient client, CallbackQuery callbackQuery, AppDbContext context, CancellationToken token)
    {
        var data = callbackQuery.Data;
        long chatId = callbackQuery.Message!.Chat.Id;
        if (data == null) return;

        if (data.StartsWith("cat_"))
        {
            int categoryId = int.Parse(data.Replace("cat_", ""));
            _draftHabits[chatId] = new Habit { CategoryId = categoryId };
            
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

        if (data.StartsWith("type_"))
        {
            int typeId = int.Parse(data.Replace("type_", ""));
            if (_draftHabits.TryGetValue(chatId, out var draft))
            {
                draft.Type = (HabitType)typeId;
                _actionStates[chatId] = BotActionState.WaitingForHabitTitle;
                await client.EditMessageText(chatId, callbackQuery.Message.MessageId, "Отлично! Теперь отправьте текстовым сообщением название привычки:", cancellationToken: token);
            }
            return;
        }

        if (data.StartsWith("dow_"))
        {
            int dayOfWeek = int.Parse(data.Replace("dow_", "")); 
            if (_draftHabits.TryGetValue(chatId, out var draft))
            {
                draft.ExecutionDayOfWeek = (DayOfWeek)dayOfWeek;
                _actionStates[chatId] = BotActionState.WaitingForReminderTime; 
                await client.EditMessageText(chatId, callbackQuery.Message.MessageId, "День установлен. Теперь введите время уведомления в формате ЧЧ:ММ (например, 08:30):", cancellationToken: token);
            }
            return;
        }

        if (data.StartsWith("done_"))
        {
            int habitId = int.Parse(data.Replace("done_", ""));
            
            await client.EditMessageReplyMarkup(chatId, callbackQuery.Message.MessageId, replyMarkup: null, cancellationToken: token);

            _actionStates[chatId] = BotActionState.WaitingForProof;
            _pendingProofHabitIds[chatId] = habitId;

            var skipKeyboard = new InlineKeyboardMarkup(
                InlineKeyboardButton.WithCallbackData("Не могу отправить", "skip_proof")
            );

            await client.SendMessage(
                chatId, 
                "Если выполнили, можете отправить фото или видео, как вы это сделали? 📸📹", 
                replyMarkup: skipKeyboard, 
                cancellationToken: token);
            return;
        }

        if (data == "skip_proof")
        {
            if (_actionStates.GetValueOrDefault(chatId) == BotActionState.WaitingForProof &&
                _pendingProofHabitIds.TryGetValue(chatId, out int habitId))
            {
                await client.EditMessageReplyMarkup(chatId, callbackQuery.Message.MessageId, replyMarkup: null, cancellationToken: token);
                
                await SaveHabitLog(client, chatId, habitId, context, "Отмечено без подтверждения", token);
                
                _actionStates[chatId] = BotActionState.None;
                _pendingProofHabitIds.TryRemove(chatId, out _);
            }
            return;
        }
    }

    private async Task SaveHabitLog(ITelegramBotClient client, long chatId, int habitId, AppDbContext context, string note, CancellationToken token)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        bool alreadyLogged = await context.HabitLogs.AnyAsync(l => l.HabitId == habitId && l.DoneDate == today, token);

        if (!alreadyLogged)
        {
            var log = new HabitLog
            {
                HabitId = habitId,
                DoneDate = today,
                Value = 1,
                Note = note
            };
            context.HabitLogs.Add(log);
            await context.SaveChangesAsync(token);

            var logs = await context.HabitLogs.Where(l => l.HabitId == habitId).Select(l => l.DoneDate).OrderByDescending(d => d).ToListAsync(token);
            int streak = CalculateStreak(logs);

            await client.SendMessage(chatId, $"Отмечено! 🔥 Серия: {streak} выполнений подряд!", cancellationToken: token);
        }
        else
        {
            await client.SendMessage(chatId, "Вы уже отмечали это сегодня!", cancellationToken: token);
        }
    }

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
        var categories = await context.Categories.ToListAsync(token);
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
        var habits = await context.Habits.Include(h => h.Logs).Where(h => h.UserId == userId && h.Status == HabitStatus.Active).ToListAsync(token);

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
            string typeEmoji = habit.Type switch
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
        var habits = await context.Habits.Include(h => h.Logs).Where(h => h.UserId == userId).ToListAsync(token);
        
        int total = habits.Count;
        int dailyCount = habits.Count(h => h.Type == HabitType.Daily);
        int weeklyCount = habits.Count(h => h.Type == HabitType.Weekly);
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int doneToday = habits.Count(h => h.Type == HabitType.Daily && h.Logs.Any(l => l.DoneDate == today));

        string stats = $"📊 <b>Ваша статистика:</b>\n\n" +
                       $"Всего привычек: {total}\n" +
                       $"Ежедневных: {dailyCount} (выполнено сегодня: {doneToday})\n" +
                       $"Еженедельных: {weeklyCount}";

        await client.SendMessage(chatId, stats, parseMode: ParseMode.Html, cancellationToken: token);
    }

    private bool CheckIfDone(Habit habit, DateOnly today)
    {
        if (!habit.Logs.Any()) return false;

        return habit.Type switch
        {
            HabitType.Daily => habit.Logs.Any(l => l.DoneDate == today),
            HabitType.Weekly => habit.Logs.Any(l => l.DoneDate >= today.AddDays(-7)),
            HabitType.Monthly => habit.Logs.Any(l => l.DoneDate.Month == today.Month && l.DoneDate.Year == today.Year),
            HabitType.Disposable => true,
            _ => false
        };
    }

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