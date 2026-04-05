using HabitTrakerApi.Models.Data;


namespace HabitTrakerApi.Repositories;

public interface IUserRepository
{
     List<User> GetListUsers();
     User? GetUser(string  login, string password);
     
     string Adduser(User user);
     string AddHabits(Habit habit, string login, string password);
     
     List<Habit> GetListMyHabits(string login , string password);
     string DeleteMyHabit(string login, string password , Habit habit);
}
public class UserRepository : IUserRepository
{
     public List<User> Users;
     public ILogger<UserRepository> _logger;

     public UserRepository( ILogger<UserRepository> logger)
     {
          User user = new User()
          {
               Login = "admin",
               Password = "123456",
               Email = "admin@gmail.com",
          };
          Users.Add(user);
          _logger = logger;
     }
     public List<User> GetListUsers()
     {
          return Users;
     }

     public User? GetUser(string login, string password)
     { 
          return Users.FirstOrDefault( u => u.Login == login && u.Password == password );
     }

     public string Adduser(User user)
     {
          Users.Add(user);
          return "пользователь успешно добавлен в базу";
     }

     public string AddHabits(Habit habit, string login, string password)
     {
          User user = Users.FirstOrDefault(u => u.Login == login && u.Password == password);
          if (user == null)
               return "такой пользователь не найден! ";
          
          user.Habits.Add(habit);


          return "привычка добавлена!";
     }

     public List<Habit> GetListMyHabits(string login, string password)
     {
          try
          {
               User? user = Users.FirstOrDefault(u => u.Login == login && u.Password == password );
               if (user == null)
                    throw new Exception("такой пользователь не найден! ");
         
               return user.Habits;
          }
          catch (Exception e)
          {
               Console.WriteLine(e);
               _logger.LogInformation($"Ощибка {e}");
               throw;
          }
         
     }

     public string DeleteMyHabit(string login, string password, Habit habit)
     {
       User?  user=    Users.FirstOrDefault(u => u.Login == login && u.Password == password);
       if (user == null)
            return "такой пользователь не существует";

       user.Habits.Remove(habit);
       
       return "привычка успешно удалена!";
     }
}