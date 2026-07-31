using HabitTrakerApi.Common;
using HabitTrakerApi.DbContext;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HabitTrakerApi.Data;

// Наполняет пустую БД тестовыми данными: пользователи, категории, привычки,
// история отметок и напоминания. Идемпотентен — если в Users уже есть записи,
// просто ничего не делает (безопасно вызывать при каждом старте приложения).
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync())
            return;

        // ---- Пользователи ----
        var admin = new User
        {
            Login = "admin",
            Email = "admin@habittraker.local",
            Password = PasswordHasher.Hash("Admin123!"),
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var ivan = new User
        {
            Login = "ivan",
            Email = "ivan@example.com",
            Password = PasswordHasher.Hash("User123!"),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow.AddDays(-25)
        };

        var olga = new User
        {
            Login = "olga",
            Email = "olga@example.com",
            Password = PasswordHasher.Hash("User123!"),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        await context.Users.AddRangeAsync(admin, ivan, olga);
        await context.SaveChangesAsync(); // нужны реальные Id для FK ниже

        // ---- Категории ----
        var health = new Category { Name = "Здоровье" };
        var sport = new Category { Name = "Спорт" };
        var productivity = new Category { Name = "Продуктивность" };
        var learning = new Category { Name = "Обучение" };
        var mindfulness = new Category { Name = "Осознанность" };

        await context.Categories.AddRangeAsync(health, sport, productivity, learning, mindfulness);
        await context.SaveChangesAsync();

        // ---- Привычки ----
        var water = new Habit
        {
            UserId = ivan.Id,
            CategoryId = health.Id,
            Title = "Пить 2 литра воды",
            Description = "8 стаканов по 250мл в течение дня",
            Type = HabitType.Daily,
            Status = HabitStatus.Active,
            Priority = PriorityLevel.High,
            TargetCount = 8,
            CreatedAt = DateTime.UtcNow.AddDays(-21)
        };

        var running = new Habit
        {
            UserId = ivan.Id,
            CategoryId = sport.Id,
            Title = "Бег по утрам",
            Description = "Минимум 3 км",
            Type = HabitType.Daily,
            Status = HabitStatus.Active,
            Priority = PriorityLevel.Medium,
            TargetCount = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-18)
        };

        var reading = new Habit
        {
            UserId = ivan.Id,
            CategoryId = learning.Id,
            Title = "Читать книгу",
            Description = "Минимум 20 страниц",
            Type = HabitType.Daily,
            Status = HabitStatus.Active,
            Priority = PriorityLevel.Medium,
            TargetCount = 20,
            CreatedAt = DateTime.UtcNow.AddDays(-14)
        };

        var meditation = new Habit
        {
            UserId = ivan.Id,
            CategoryId = mindfulness.Id,
            Title = "Медитация",
            Description = "10 минут перед сном",
            Type = HabitType.Daily,
            Status = HabitStatus.Paused,
            Priority = PriorityLevel.Low,
            TargetCount = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var cleaning = new Habit
        {
            UserId = olga.Id,
            CategoryId = productivity.Id,
            Title = "Генеральная уборка",
            Type = HabitType.Weekly,
            Status = HabitStatus.Active,
            Priority = PriorityLevel.Medium,
            TargetCount = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-40)
        };

        var yoga = new Habit
        {
            UserId = olga.Id,
            CategoryId = sport.Id,
            Title = "Йога",
            Description = "Два занятия в неделю",
            Type = HabitType.Weekly,
            Status = HabitStatus.Active,
            Priority = PriorityLevel.High,
            TargetCount = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-35)
        };

        var words = new Habit
        {
            UserId = olga.Id,
            CategoryId = learning.Id,
            Title = "Учить английские слова",
            Description = "10 новых слов в день",
            Type = HabitType.Daily,
            Status = HabitStatus.Active,
            Priority = PriorityLevel.Medium,
            TargetCount = 10,
            CreatedAt = DateTime.UtcNow.AddDays(-12)
        };

        var declutter = new Habit
        {
            UserId = olga.Id,
            CategoryId = productivity.Id,
            Title = "Разбор шкафа",
            Description = "Разовая задача",
            Type = HabitType.Disposable,
            Status = HabitStatus.Completed,
            Priority = PriorityLevel.Low,
            TargetCount = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-9)
        };

        await context.Habits.AddRangeAsync(water, running, reading, meditation, cleaning, yoga, words, declutter);
        await context.SaveChangesAsync();

        // ---- Записи о выполнении (HabitLog) ----
        // Для ежедневных привычек генерируем историю за последние N дней с редкими
        // пропусками, чтобы стрики и % выполнения выглядели реалистично, а не как 100%.
        var logs = new List<HabitLog>();

        logs.AddRange(GenerateDailyLogs(water.Id, days: 14, targetCount: 8, skipEvery: 5));
        logs.AddRange(GenerateDailyLogs(running.Id, days: 14, targetCount: 1, skipEvery: 4));
        logs.AddRange(GenerateDailyLogs(reading.Id, days: 10, targetCount: 20, skipEvery: 6, valueVariance: true));
        logs.AddRange(GenerateDailyLogs(words.Id, days: 10, targetCount: 10, skipEvery: 7));

        // Йога — недельная привычка, отмечаем вручную пару занятий
        logs.Add(new HabitLog { HabitId = yoga.Id, DoneDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), Value = 1, Note = "Утреннее занятие" });
        logs.Add(new HabitLog { HabitId = yoga.Id, DoneDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)), Value = 1 });

        logs.Add(new HabitLog { HabitId = cleaning.Id, DoneDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)), Value = 1 });
        logs.Add(new HabitLog { HabitId = declutter.Id, DoneDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-9)), Value = 1, Note = "Выполнено полностью" });

        await context.HabitLogs.AddRangeAsync(logs);
        await context.SaveChangesAsync();

        // ---- Напоминания ----
        var reminders = new List<Reminder>
        {
            new() { HabitId = water.Id, ReminderTime = new TimeOnly(9, 0), IsEnabled = true },
            new() { HabitId = water.Id, ReminderTime = new TimeOnly(15, 0), IsEnabled = true },
            new() { HabitId = running.Id, ReminderTime = new TimeOnly(7, 0), IsEnabled = true },
            new() { HabitId = reading.Id, ReminderTime = new TimeOnly(21, 30), IsEnabled = true },
            new() { HabitId = meditation.Id, ReminderTime = new TimeOnly(22, 0), IsEnabled = false },
            new() { HabitId = words.Id, ReminderTime = new TimeOnly(8, 30), IsEnabled = true },
            new() { HabitId = yoga.Id, ReminderTime = new TimeOnly(18, 0), IsEnabled = true }
        };

        await context.Reminders.AddRangeAsync(reminders);
        await context.SaveChangesAsync();
    }

    // Детерминированный (seed = habitId) генератор логов, чтобы при повторной пересборке
    // проекта данные получались одинаковыми, а не случайными при каждом запуске.
    private static IEnumerable<HabitLog> GenerateDailyLogs(int habitId, int days, int targetCount, int skipEvery, bool valueVariance = false)
    {
        var random = new Random(habitId);

        for (int i = 1; i <= days; i++)
        {
            if (i % skipEvery == 0)
                continue; // имитация пропущенного дня

            var value = targetCount;
            if (valueVariance)
                value = Math.Max(1, targetCount + random.Next(-5, 10));

            yield return new HabitLog
            {
                HabitId = habitId,
                DoneDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i)),
                Value = value
            };
        }
    }
}
