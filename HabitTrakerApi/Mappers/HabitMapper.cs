using HabitTrakerApi.DTO.Habits;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Models.Enums;

namespace HabitTrakerApi.Mappers;

public static class HabitMapper
{
    public static HabitDto ToDto(this Habit habit, int currentStreak = 0, int longestStreak = 0, double completionRate = 0)
    {
        return new HabitDto
        {
            Id = habit.Id,
            UserId = habit.UserId,
            CategoryId = habit.CategoryId,
            CategoryName = habit.Category?.Name ?? string.Empty,
            Title = habit.Title,
            Description = habit.Description,
            Type = habit.Type,
            Status = habit.Status,
            Priority = habit.Priority,
            TargetCount = habit.TargetCount,
            CreatedAt = habit.CreatedAt,
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            CompletionRate = completionRate,
            ExecutionDayOfWeek = habit.ExecutionDayOfWeek,
            ExecutionDayOfMonth = habit.ExecutionDayOfMonth
        };
    }

    public static Habit ToEntity(this CreateHabitDto dto, int userId)
    {
        return new Habit
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            Title = dto.Title.Trim(),
            Description = dto.Description,
            Type = dto.Type,
            Priority = dto.Priority,
            TargetCount = dto.TargetCount,
            Status = HabitStatus.Active,
            CreatedAt = DateTime.UtcNow,
            ExecutionDayOfWeek = dto.ExecutionDayOfWeek,
            ExecutionDayOfMonth = dto.ExecutionDayOfMonth
        };
    }

    public static void ApplyUpdate(this Habit habit, UpdateHabitDto dto)
    {
        if (dto.CategoryId.HasValue) habit.CategoryId = dto.CategoryId.Value;
        if (!string.IsNullOrWhiteSpace(dto.Title)) habit.Title = dto.Title.Trim();
        if (dto.Description is not null) habit.Description = dto.Description;
        if (dto.Type.HasValue) habit.Type = dto.Type.Value;
        if (dto.Priority.HasValue) habit.Priority = dto.Priority.Value;
        if (dto.TargetCount.HasValue) habit.TargetCount = dto.TargetCount.Value;
        if (dto.ExecutionDayOfWeek.HasValue) habit.ExecutionDayOfWeek = dto.ExecutionDayOfWeek.Value;
        if (dto.ExecutionDayOfMonth.HasValue) habit.ExecutionDayOfMonth = dto.ExecutionDayOfMonth.Value;
    }
}