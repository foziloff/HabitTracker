using System.ComponentModel.DataAnnotations;

namespace HabitTrakerApi.DTOs.Reminders;

public class ReminderDto
{
    public int Id { get; set; }
    public int HabitId { get; set; }
    public TimeOnly ReminderTime { get; set; }
    public bool IsEnabled { get; set; }
}

public class CreateReminderDto
{
    [Required]
    public TimeOnly ReminderTime { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class UpdateReminderDto
{
    public TimeOnly? ReminderTime { get; set; }
    public bool? IsEnabled { get; set; }
}
