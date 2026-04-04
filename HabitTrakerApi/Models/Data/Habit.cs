using System.ComponentModel.DataAnnotations;
using HabitTrakerApi.Models.Enums;

namespace HabitTrakerApi.Models.Data;


public class Habit : EntityBase
{
    public string Title { get; set; }
    public HabitType Type { get; set; }

    public int UserId { get; set; }

    public bool IsCompleted { get; set; }
}