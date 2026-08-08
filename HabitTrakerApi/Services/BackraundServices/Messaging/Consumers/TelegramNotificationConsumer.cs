using HabitTrakerApi.Messaging.Events;
using HabitTrakerApi.Services.BackraundServices;
using MassTransit;

namespace HabitTrakerApi.Messaging.Consumers
{
    // Получает событие от NotificationService и реально отправляет сообщение через TelegramService.
    // Больше ничего не делает — вся логика "кому и когда" осталась в NotificationService.
    public class TelegramNotificationConsumer : IConsumer<HabitReminderDueEvent>
    {
        private readonly TelegramService _telegramService;
        private readonly ILogger<TelegramNotificationConsumer> _logger;

        public TelegramNotificationConsumer(TelegramService telegramService, ILogger<TelegramNotificationConsumer> logger)
        {
            _telegramService = telegramService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<HabitReminderDueEvent> context)
        {
            var msg = context.Message;

            try
            {
                await _telegramService.SendMessageAsync(msg.ChatId, msg.Text);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось отправить сообщение в Telegram (chatId={ChatId})", msg.ChatId);
            }
        }
    }
}
