using HabitTrakerApi.DTOs.HabitLogs;
using HabitTrakerApi.Models.Data;

namespace HabitTrakerApi.Mappers;

public static class HabitLogMapper
{
    public static HabitLogDto ToDto(this HabitLog log)
    {
        return new HabitLogDto
        {
            Id = log.Id,
            HabitId = log.HabitId,
            DoneDate = log.DoneDate,
            Value = log.Value,
            Note = log.Note
        };
    }

    public static HabitLog ToEntity(this CreateHabitLogDto dto, int habitId)
    {
        return new HabitLog
        {
            HabitId = habitId,
            DoneDate = dto.DoneDate,
            Value = dto.Value,
            Note = dto.Note
        };
    }

    public static void ApplyUpdate(this HabitLog log, UpdateHabitLogDto dto)
    {
        if (dto.Value.HasValue) log.Value = dto.Value.Value;
        if (dto.Note is not null) log.Note = dto.Note;
    }
}
