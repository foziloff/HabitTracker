using System.ComponentModel.DataAnnotations;
using HabitTrakerApi.DTO.Users;

namespace HabitTrakerApi.DTO.Auth;

public class RegisterDto
{
    [Required, MinLength(3), MaxLength(50)]
    public string Login { get; set; } = null!;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = null!;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = null!;
}