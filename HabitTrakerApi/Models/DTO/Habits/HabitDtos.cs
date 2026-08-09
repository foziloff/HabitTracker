using System.ComponentModel.DataAnnotations;
using HabitTrakerApi.Models.Enums;

namespace HabitTrakerApi.DTO.Habits;

public class HabitDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public HabitType Type { get; set; }
    public HabitStatus Status { get; set; }
    public PriorityLevel Priority { get; set; }
    public int TargetCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public double CompletionRate { get; set; }

    // Актуально только для Type == Weekly
    public DayOfWeek? ExecutionDayOfWeek { get; set; }

    // Актуально только для Type == Monthly
    public int? ExecutionDayOfMonth { get; set; }
}

public class CreateHabitDto
{
    [Required]
    public int CategoryId { get; set; }

    [Required, MaxLength(150)]
    public string Title { get; set; } = null!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public HabitType Type { get; set; }

    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

    [Range(1, int.MaxValue, ErrorMessage = "TargetCount должен быть больше нуля")]
    public int TargetCount { get; set; } = 1;

    // Обязательно, если Type == Weekly. Проверяется в HabitService, не здесь,
    // т.к. DataAnnotations не умеют "обязательно только при определённом Type".
    public DayOfWeek? ExecutionDayOfWeek { get; set; }

    // Обязательно, если Type == Monthly.
    [Range(1, 31, ErrorMessage = "ExecutionDayOfMonth должен быть от 1 до 31")]
    public int? ExecutionDayOfMonth { get; set; }
}

public class UpdateHabitDto
{
    public int? CategoryId { get; set; }

    [MaxLength(150)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public HabitType? Type { get; set; }

    public PriorityLevel? Priority { get; set; }

    [Range(1, int.MaxValue)]
    public int? TargetCount { get; set; }

    public DayOfWeek? ExecutionDayOfWeek { get; set; }

    [Range(1, 31, ErrorMessage = "ExecutionDayOfMonth должен быть от 1 до 31")]
    public int? ExecutionDayOfMonth { get; set; }
}

public class UpdateHabitStatusDto
{
    [Required]
    public HabitStatus Status { get; set; }
}

public class HabitStatsDto
{
    public int HabitId { get; set; }
    public string Title { get; set; } = null!;
    public int TotalLogs { get; set; }
    public int TotalValue { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public double CompletionRate { get; set; }
    public DateOnly? LastDoneDate { get; set; }
}

// Параметры фильтрации/пагинации для GET /api/habits
public class HabitQueryParams
{
    public int? CategoryId { get; set; }
    public HabitStatus? Status { get; set; }
    public HabitType? Type { get; set; }
    public PriorityLevel? Priority { get; set; }
    public string? Search { get; set; }

    private int _page = 1;
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is < 1 or > 100 ? 20 : value;
    }
}