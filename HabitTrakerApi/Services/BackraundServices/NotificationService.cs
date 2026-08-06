using Telegram.Bot;

namespace HabitTrakerApi.Services.BackraundServices
{
    public class NotificationService : BackgroundService
    {
        private TelegramService _telegramService;
        public NotificationService(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _telegramService.SendMessageAsync(123456789, "Hello from NotificationService");


            throw new NotImplementedException();
        }
    }
}
