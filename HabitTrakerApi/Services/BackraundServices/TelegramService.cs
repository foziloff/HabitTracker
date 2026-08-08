using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using System.Collections.Concurrent;
using HabitTrakerApi.Models.Enums;
using HabitTrakerApi.DbContext;
using HabitTrakerApi.Common;
using Microsoft.EntityFrameworkCore;

namespace HabitTrakerApi.Services.BackraundServices;

public class TelegramService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    // Используем ConcurrentDictionary для потокобезопасности в Singleton
    private readonly ConcurrentDictionary<long, TelegramAuthState> _chats = new();
    private readonly TelegramBotClient _telegramBotClient;
    private readonly ConcurrentDictionary<long, string> CurrentUser = new();

    public TelegramService(IServiceScopeFactory scope)
    {
        // В реальном проекте токен лучше вынести в appsettings.json
        _telegramBotClient = new TelegramBotClient("8821028314:AAHh_tPx9mWs3ZP8LDmDxCUJNlzoGXOZKyk");
        _scopeFactory = scope;
    }

    // Этот метод .NET автоматически вызывает при запуске приложения
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [] // Получать все типы обновлений
        };

        _telegramBotClient.StartReceiving(
            updateHandler: Update,
            errorHandler: Error,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );

        // Ждем сигнала остановки приложения
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    private async Task Update(ITelegramBotClient client, Update update, CancellationToken token)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            // 3. Достаем AppDbContext внутри этого Scope
            var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 4. Теперь здесь безопасно работаем с базой данных
            // Пример: var habits = await dbContext.Habits.ToListAsync(stoppingToken);
             
            var message = update.Message;
            if (string.IsNullOrEmpty(message?.Text)) return;

            if (message.Text.Equals("/start", StringComparison.OrdinalIgnoreCase))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.ChatId == message.Chat.Id);
                if (user is not null)
                {
                    _chats[message.Chat.Id] = TelegramAuthState.Authorized;
                    await client.SendMessage(message.Chat.Id, "Привет! Вы уже авторизованный польователь", cancellationToken: token);
                    return;
                }
                // Используем [chatId] = value, чтобы не падать при повторном /start
                _chats[message.Chat.Id] = TelegramAuthState.None;

                await client.SendMessage(message.Chat.Id, "Привет! Пожалуйста, авторизуйтесь", cancellationToken: token);

                ReplyKeyboardMarkup replyKeyboards = new(new[]
                {
                new KeyboardButton[] { "/auth" }
            })
                {
                    ResizeKeyboard = true
                };
                await client.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Выберите действие:",
                    replyMarkup: replyKeyboards,
                    cancellationToken: token
                );
                return;

            }
            if (message.Text.ToLower() == "/auth")
            {
                _chats[message.Chat.Id] = TelegramAuthState.WaitingLogin;
                await client.SendMessage(message.Chat.Id, "Отлично! введите логин", cancellationToken: token);
                return;
            }
            if (_chats.TryGetValue(message.Chat.Id, out var state) && state == TelegramAuthState.WaitingLogin)
            {
                HabitTrakerApi.Models.Data.User? user = await _context.Users.FirstOrDefaultAsync(u => u.Login == message.Text);
                if (user is null)
                {
                    _chats[message.Chat.Id] = TelegramAuthState.None;
                    await client.SendMessage(message.Chat.Id, "Неверный логин!", cancellationToken: token);
                    return;
                }
                _chats[message.Chat.Id] = TelegramAuthState.WaitingPassword;
                CurrentUser[message.Chat.Id] = user.Login;

                await client.SendMessage(message.Chat.Id, "Ведите свой пароль", cancellationToken: token);
                return;
            }

            if (_chats.TryGetValue(message.Chat.Id, out var stat) && stat == TelegramAuthState.WaitingPassword)
            {
                var user1 =await _context.Users.AsTracking().FirstOrDefaultAsync(u => u.Login == CurrentUser[message.Chat.Id]);


                if (PasswordHasher.Verify(message.Text?.Trim() ?? string.Empty, user1.Password))
                {
                     await client.SendMessage(message.Chat.Id, "Вы успешно авторизовались,", cancellationToken: token);
                    user1.ChatId = message.Chat.Id;

                    _chats[message.Chat.Id] = TelegramAuthState.Authorized;
                    await _context.SaveChangesAsync();

                    return;

                }
                else
                {

                    _chats[message.Chat.Id] = TelegramAuthState.None;
                    await client.SendMessage(message.Chat.Id, "Неверный пароль!", cancellationToken: token);
                    return;
                }
            }

        }
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