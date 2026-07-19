using HabitTrakerApi.Models.Enums;

namespace HabitTrakerApi.Models.DTO;

public class CreateHabitDto
{
    public string Title { get; set; }

    public HabitType Type { get; set; }

    public PriorityLevel Priority { get; set; }

    public int TargetCount { get; set; }

    public int CategoryId { get; set; }

}