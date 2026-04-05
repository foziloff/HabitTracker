using HabitTrakerApi.Models.Data;

namespace HabitTrakerApi.Models.DTO;

public class UserDto
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string? Email { get; set; }
    public List<Habit> Habits { get; set; } = new List<Habit>();
}