using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Models.DTO;

namespace HabitTrakerApi.Services;

public interface IServiceHabits
{
    List<Habit> GetAllHabits();
    Habit GetHabitById(int id);
    Habit CreateHabit(Habit habit);
    Habit UpdateHabit(int id, Habit habit);
    void DeleteHabit(int id);
    HabitLog AddHabitLog(HabitLog log);
    Reminder AddReminder(Reminder reminder);
}