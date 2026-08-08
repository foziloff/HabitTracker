using System.ComponentModel.DataAnnotations;
using HabitTrakerApi.Models.Enums;

namespace HabitTrakerApi.Models.Data;

public class Habit :EntityBase
{public int UserId { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public HabitType Type { get; set; }
    public HabitStatus Status { get; set; }
    public PriorityLevel Priority { get; set; }
    public int TargetCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DayOfWeek? ExecutionDayOfWeek { get; set; }
    public int? ExecutionDayOfMonth { get; set; }

    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<HabitLog> Logs { get; set; } = new List<HabitLog>();
    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
}