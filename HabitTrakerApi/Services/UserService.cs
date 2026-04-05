using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Repositories;

namespace HabitTrakerApi.Services;

public interface IUserService
{
    string Add(User newUser);
    List<User> GetUsers();
    User  Autorization(string login , string password);
}

public class UserService :IUserService
{
    private IUserRepository _repository;

    public UserService(IUserRepository  repository)
    {
            _repository = repository;
    }
    public string Add(User newUser)
    {
      string  response = _repository.Adduser( newUser);
      return response;
    }

    public List<User> GetUsers()
    {
      List<User> users =  _repository.GetListUsers();
      return users;
    }

    public User Autorization(string login, string password)
    {
      User user =  _repository.GetUser(login, password);
      return user;
    }
}