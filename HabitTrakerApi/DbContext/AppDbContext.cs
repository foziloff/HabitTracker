using HabitTrakerApi.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace HabitTrakerApi.DbContext;

public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<HabitLog> HabitLogs => Set<HabitLog>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Уникальный индекс для Email
        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        // Связь: Пользователь -> Привычки
        modelBuilder.Entity<Habit>()
            .HasOne(x => x.User)
            .WithMany(x => x.Habits)
            .HasForeignKey(x => x.UserId);

        // Связь: Категория -> Привычки (Исправлено здесь)
        modelBuilder.Entity<Habit>()
            .HasOne(x => x.Category)
            .WithMany(x => x.Habits)
            .HasForeignKey(x => x.CategoryId);

        // Связь: Привычка -> Логи выполнения
        modelBuilder.Entity<HabitLog>()
            .HasOne(x => x.Habit)
            .WithMany(x => x.Logs)
            .HasForeignKey(x => x.HabitId);

        // Связь: Привычка -> Напоминания
        modelBuilder.Entity<Reminder>()
            .HasOne(x => x.Habit)
            .WithMany(x => x.Reminders)
            .HasForeignKey(x => x.HabitId);
    }
}
