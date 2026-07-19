using HabitTrakerApi.Models.Enums;

namespace HabitTrakerApi.Models.DTO;

public class HabitDto
{
        public int Id { get; set; }
        public string Title { get; set; }
        public HabitType Type { get; set; }
        public bool IsCompleted { get; set; }
}