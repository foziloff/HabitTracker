using HabitTrakerApi.DbContext;

namespace HabitTrakerApi.Repositories;
using HabitTrakerApi.Models.Data;

public interface IHabitTrackerRepository
{
    public List<Habit> GetAllHabits();
    public Habit AddHabit(Habit habit);
    public Habit UpdateHabit(int id, Habit habit);
    public Habit? GetHabitById(int id);
    public void DeleteHabit(int id);
    public HabitLog AddLog(HabitLog log);
    public Reminder AddReminder(Reminder reminder);

}

public class HabitTrackerRepository : IHabitTrackerRepository
{
    private readonly AppDbContext _context;

    public HabitTrackerRepository( AppDbContext context)
    {
        _context = context;
    }

    public List<Habit> GetAllHabits()
    {
        return  _context.Habits.ToList();
    }

    public Habit? GetHabitById(int id)
    {
        return _context.Habits.FirstOrDefault(x => x.Id == id);
    }

    public Habit AddHabit(Habit habit)
    {
        _context.Habits.AddAsync(habit);
        return habit;
    }

    public Habit UpdateHabit(int id, Habit habit)
    {
        var existing = _context.Habits.FirstOrDefault(x => x.Id == id);

        if (existing == null)
            throw new Exception("Habit not found");

        existing.Title = habit.Title;
        existing.Description = habit.Description;
        existing.Type = habit.Type;
        existing.Status = habit.Status;
        existing.Priority = habit.Priority;
        existing.TargetCount = habit.TargetCount;
        existing.CategoryId = habit.CategoryId;

        return existing;
    }

    public void DeleteHabit(int id)
    {
        var habit = _context.Habits.FirstOrDefault(x => x.Id == id);

        if (habit == null)
            throw new Exception("Habit not found");

        _context.Habits.Remove(habit);
    }

    public HabitLog AddLog(HabitLog log)
    {
        _context.HabitLogs.Add(log);
        return log;
    }

    public Reminder AddReminder(Reminder reminder)
    {
        _context.Reminders.Add(reminder);
        return reminder;
    }
}