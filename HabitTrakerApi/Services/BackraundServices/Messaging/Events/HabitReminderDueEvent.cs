namespace HabitTrakerApi.Messaging.Events
{
    // Событие "пора напомнить пользователю о привычке". NotificationService публикует,
    // TelegramNotificationConsumer — единственный, кто его слушает.
    public record HabitReminderDueEvent
    {
        public long ChatId { get; init; }
        public string Text { get; init; } = null!;
    }
}
