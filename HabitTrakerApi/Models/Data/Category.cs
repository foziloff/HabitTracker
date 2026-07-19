namespace HabitTrakerApi.Models.Data;

public class Category :EntityBase
{

    public string Name { get; set; } = null!;

    public ICollection<Habit> Habits { get; set; } = new List<Habit>();
}