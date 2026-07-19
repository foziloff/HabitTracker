namespace HabitTrakerApi.Models.DTO;

public class CheckUserDto
{
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = string.Empty;


}