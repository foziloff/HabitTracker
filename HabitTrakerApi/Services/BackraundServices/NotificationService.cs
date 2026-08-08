using HabitTrakerApi.Data;
using HabitTrakerApi.DbContext;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HabitTrakerApi.Services.BackraundServices
{
    public class NotificationService : BackgroundService
    {
        private readonly TelegramService _telegramService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationService> _logger;

        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

        public NotificationService(
            TelegramService telegramService,
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationService> logger)
        {
            _telegramService = telegramService;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(CheckInterval);

            do
            {
                try
                {
                    await CheckHabitsAndNotifyAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при проверке привычек для уведомлений");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task CheckHabitsAndNotifyAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            // ВАЖНО: если ваш DbContext называется иначе, чем AppDbContext — замените имя здесь.
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var nowDateTime = DateTime.Now;
            var now = TimeOnly.FromDateTime(nowDateTime);

            // Время у напоминания совпало — это общий фильтр для всех типов привычек.
            var remindersAtThisTime = await db.Reminders
                .Include(r => r.Habit)
                    .ThenInclude(h => h.User)
                .Where(r => r.IsEnabled
                            && r.Habit.Status == HabitStatus.Active
                            && r.ReminderTime.Hour == now.Hour
                            && r.ReminderTime.Minute == now.Minute
                            && r.Habit.User.ChatId != null)
                .ToListAsync(stoppingToken);

            
            var dueReminders = remindersAtThisTime
                .Where(r => IsDueToday(r.Habit, nowDateTime))
                .ToList();

            foreach (var reminder in dueReminders)
            {
                var habit = reminder.Habit;
                var chatId = habit.User.ChatId!.Value;

                var text = $"⏰ Пора выполнить привычку «{habit.Title}» ({GetTypeLabel(habit.Type)})";

                await _telegramService.SendMessageAsync(chatId, text);
            }

            if (dueReminders.Count > 0)
                _logger.LogInformation("Отправлено {Count} уведомлений на {Time:HH:mm}", dueReminders.Count, now);
        }

        // Daily — каждый день. Weekly — только в тот же день недели, когда привычка создана.
        // Monthly — только в тот же день месяца (с поправкой на короткие месяцы). Disposable —
        // каждый день, пока привычка не выполнена и не переведена в Completed/Archived вручную.
        private static bool IsDueToday(Habit habit, DateTime now)
        {
            return habit.Type switch
            {
                HabitType.Daily => true,
                HabitType.Disposable => true,
                HabitType.Weekly => now.DayOfWeek == habit.CreatedAt.DayOfWeek,
                HabitType.Monthly => now.Day == GetAnchorDayClamped(habit.CreatedAt, now),
                _ => true
            };
        }

        private static int GetAnchorDayClamped(DateTime createdAt, DateTime now)
        {
            var daysInCurrentMonth = DateTime.DaysInMonth(now.Year, now.Month);
            return Math.Min(createdAt.Day, daysInCurrentMonth);
        }

        private static string GetTypeLabel(HabitType type) => type switch
        {
            HabitType.Daily => "ежедневная",
            HabitType.Weekly => "еженедельная",
            HabitType.Monthly => "ежемесячная",
            HabitType.Disposable => "разовая",
            _ => type.ToString()
        };
    }
}