using HabitTrakerApi.Models.Data;

namespace HabitTrakerApi.Repositories;

public interface IHabitRepository
{
    void Add(Habit habit);
    Habit GetById(int id);
    List<Habit> GetByUserId(int userId);
    List<Habit> GetAll();
}
public class HabitRepository : IHabitRepository
{
    private readonly List<Habit> _habits = new();
    private int _id = 1;

    public void Add(Habit habit)
    {
        habit.id = _id++;
        _habits.Add(habit);
    }

    public Habit? GetById(int id)
        => _habits.FirstOrDefault(x => x.id == id);

    public List<Habit> GetByUserId(int userId)
        => _habits.Where(x => x.UserId == userId).ToList();

    public List<Habit> GetAll() => _habits;
}