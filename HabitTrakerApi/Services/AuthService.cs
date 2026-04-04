using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Models.DTO;
using HabitTrakerApi.Models.Enums;
using HabitTrakerApi.Repositories;

namespace HabitTrakerApi.Services;

public interface IAuthService
{
    string Register(RegisterDto dto);
    string Login(LoginDto dto);
    User GetCurrent(string token);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository repo, ILogger<AuthService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public string Register(RegisterDto dto)
    {
        if (_repo.GetByLogin(dto.Login) != null)
            throw new Exception("User exists");

        var user = new User
        {
            Login = dto.Login,
            Password = dto.Password
        };

        _repo.Add(user);
        _logger.LogInformation("User registered {Login}", dto.Login);

        return GenerateToken(user);
    }

    public string Login(LoginDto dto)
    {
        var user = _repo.GetByLogin(dto.Login);

        if (user == null || user.Password != dto.Password)
            throw new Exception("Invalid credentials");

        _logger.LogInformation("User login {Login}", dto.Login);

        return GenerateToken(user);
    }

    private string GenerateToken(User user)
    {
        var token = Guid.NewGuid().ToString();
        user.Token = token;
        return token;
    }

    public User GetCurrent(string token)
    {
        var user = _repo.GetByToken(token);
        if (user == null)
            throw new Exception("Unauthorized");

        return user;
    }
}