using HabitTrakerApi.Models.Data;


namespace HabitTrakerApi.Repositories;

public interface IUserRepository
{
    User GetByLogin(string login);
    User GetByToken(string token);
    void Add(User user);
    List<User> GetAll();
}
public class UserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private int _id = 1;

    public void Add(User user)
    {
        user.id = _id++;
        _users.Add(user);
    }

    public User? GetByLogin(string login)
        => _users.FirstOrDefault(x => x.Login == login);

    public User? GetByToken(string token)
        => _users.FirstOrDefault(x => x.Token == token);

    public List<User> GetAll() => _users;
}