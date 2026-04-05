using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Repositories;

namespace HabitTrakerApi.Services;

interface IHabitService
{
   List<Habit> GetMyHabits( string login, string password);
   string AddHabit(Habit habit, string  login, string password);
   string DeleteHabit(Habit habit, string  login, string password);
}

public class HabitService : IHabitService
{
   private IUserRepository _repository;

   public HabitService(IUserRepository repository)
   {
      _repository = repository;
   }
   public List<Habit> GetMyHabits(string login, string password)
   {
      return _repository.GetListMyHabits( login, password);
   }

   public string AddHabit(Habit habit, string login, string password)
   {
     return _repository.AddHabits(habit, login, password);
   }

   public string DeleteHabit(Habit habit, string login, string password)
   {
     return _repository.DeleteMyHabit(login, password, habit);
     
   }
}