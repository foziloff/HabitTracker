using HabitTrakerApi.Models.Enums;

namespace HabitTrakerApi.Models.DTO;
public class CreateHabitDto
{
    public string Title { get; set; }
    public HabitType Type { get; set; }
    public int UserId { get; set; }
}