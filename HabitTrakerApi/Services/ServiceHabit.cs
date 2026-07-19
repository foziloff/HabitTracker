using HabitTrakerApi.DbContext;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HabitTrakerApi.Services;

public class ServiceHabit : IServiceHabits
{
    private readonly IHabitTrackerRepository _repository;

    public ServiceHabit(IHabitTrackerRepository repository)
    {
        _repository = repository;
    }

    public List<Habit> GetAllHabits()
    {
        return _repository.GetAllHabits();
    }

    public Habit GetHabitById(int id)
    {
        var habit = _repository.GetHabitById(id);

        if (habit == null)
            throw new Exception("Habit not found");

        return habit;
    }

    public Habit CreateHabit(Habit habit)
    {
        habit.CreatedAt = DateTime.UtcNow;
        return _repository.AddHabit(habit);
    }

    public Habit UpdateHabit(int id, Habit habit)
    {
        return _repository.UpdateHabit(id, habit);
    }

    public void DeleteHabit(int id)
    {
        _repository.DeleteHabit(id);
    }

    public HabitLog AddHabitLog(HabitLog log)
    {
        log.DoneDate = DateOnly.FromDateTime(DateTime.UtcNow);
        return _repository.AddLog(log);
    }

    public Reminder AddReminder(Reminder reminder)
    {
        return _repository.AddReminder(reminder);
    }
}