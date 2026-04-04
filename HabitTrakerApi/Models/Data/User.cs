using HabitTrakerApi.Models.Enums;

namespace HabitTrakerApi.Models.Data;

public class User : EntityBase
{
    public string Login { get; set; }
    public string Password { get; set; }

    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string Token { get; set; }

    public UserRole Role { get; set; } = UserRole.User;
}