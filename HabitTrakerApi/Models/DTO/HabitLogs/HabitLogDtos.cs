using System.ComponentModel.DataAnnotations;

namespace HabitTrakerApi.DTOs.HabitLogs;

public class HabitLogDto
{
    public int Id { get; set; }
    public int HabitId { get; set; }
    public DateOnly DoneDate { get; set; }
    public int Value { get; set; }
    public string? Note { get; set; }
}

public class CreateHabitLogDto
{
    [Required]
    public DateOnly DoneDate { get; set; }

    [Range(1, int.MaxValue)]
    public int Value { get; set; } = 1;

    [MaxLength(500)]
    public string? Note { get; set; }
}

public class UpdateHabitLogDto
{
    [Range(1, int.MaxValue)]
    public int? Value { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}
