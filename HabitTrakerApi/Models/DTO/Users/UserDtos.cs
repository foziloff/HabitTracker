using System.ComponentModel.DataAnnotations;
using HabitTrakerApi.Models.Enums;

namespace HabitTrakerApi.DTO.Users;

public class UserDto
{
    public int Id { get; set; }
    public string Login { get; set; } = null!;
    public string Email { get; set; } = null!;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public int HabitsCount { get; set; }
}

public class UpdateUserDto
{
    [MaxLength(50)]
    public string? Login { get; set; }

    [EmailAddress, MaxLength(150)]
    public string? Email { get; set; }
}

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = null!;

    [Required, MinLength(6), MaxLength(100)]
    public string NewPassword { get; set; } = null!;
}

public class UpdateUserRoleDto
{
    [Required]
    public UserRole Role { get; set; }
}
