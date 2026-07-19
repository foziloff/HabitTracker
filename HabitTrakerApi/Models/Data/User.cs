    using HabitTrakerApi.Models.Enums;

namespace HabitTrakerApi.Models.Data;

public class User:EntityBase
{

    public string Login { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public UserRole Role { get; set; } = UserRole.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Habit> Habits { get; set; } = new List<Habit>();
}