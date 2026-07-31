using HabitTrakerApi.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Internal;

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
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<Habit>()
            .HasOne(x => x.User)
            .WithMany(x => x.Habits)
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<Habit>()
            .
            HasOne(x => x.Category)
            .WithMany(x => x.Habits)
            .HasForeignKey(x => x.CategoryId);

        modelBuilder.Entity<HabitLog>()
            .HasOne(x => x.Habit)
            .WithMany(x => x.Logs)
            .HasForeignKey(x => x.HabitId);

        modelBuilder.Entity<Reminder>()
            .HasOne(x => x.Habit)
            .WithMany(x => x.Reminders)
            .HasForeignKey(x => x.HabitId);
    }
}