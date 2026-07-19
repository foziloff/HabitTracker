using System.ComponentModel.DataAnnotations;
using HabitTrakerApi.DTOs.Users;

namespace HabitTrakerApi.DTOs.Auth;

public class RegisterDto
{
    [Required, MinLength(3), MaxLength(50)]
    public string Login { get; set; } = null!;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = null!;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = null!;
}

public class LoginDto
{
    [Required]
    public string Login { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}
