using HabitTrakerApi.DTO.Reminders;
using HabitTrakerApi.Models.Data;

namespace HabitTrakerApi.Mappers;

public static class ReminderMapper
{
    public static ReminderDto ToDto(this Reminder reminder)
    {
        return new ReminderDto
        {
            Id = reminder.Id,
            HabitId = reminder.HabitId,
            ReminderTime = reminder.ReminderTime,
            IsEnabled = reminder.IsEnabled
        };
    }

    public static Reminder ToEntity(this CreateReminderDto dto, int habitId)
    {
        return new Reminder
        {
            HabitId = habitId,
            ReminderTime = dto.ReminderTime,
            IsEnabled = dto.IsEnabled
        };
    }

    public static void ApplyUpdate(this Reminder reminder, UpdateReminderDto dto)
    {
        if (dto.ReminderTime.HasValue) reminder.ReminderTime = dto.ReminderTime.Value;
        if (dto.IsEnabled.HasValue) reminder.IsEnabled = dto.IsEnabled.Value;
    }
}
