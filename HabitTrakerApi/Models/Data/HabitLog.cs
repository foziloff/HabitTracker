namespace HabitTrakerApi.Models.Data;

public class HabitLog :EntityBase
{

    public int HabitId { get; set; }

    public DateOnly DoneDate { get; set; }

    public int Value { get; set; }

    public string? Note { get; set; }

    public Habit Habit { get; set; } = null!;
}