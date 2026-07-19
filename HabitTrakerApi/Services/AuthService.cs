using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HabitTrakerApi.DTO.Auth;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Repositories;
using HabitTrakerApi.Repositories.Interfaces;
using HabitTrakerApi.Common;
using HabitTrakerApi.Common.Exeptions;
using Microsoft.IdentityModel.Tokens;

namespace HabitTrakerApi.Services;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterDto dto);
    Task<string> LoginService(RegisterDto dto);
    Task<string> GenereticJwtToken(User user);
}
public class AuthServiceJwt : IAuthService
{
    private readonly IJwtServiceRepository JwtRepository;
    private readonly IConfiguration _config;
    private readonly IUserRepository _userRepository;


    public AuthServiceJwt(IJwtServiceRepository jwtRepository, IConfiguration config ,IUserRepository userRepository)
    {
        JwtRepository = jwtRepository;
        _config = config;
        _userRepository = userRepository;
    }
    
    public async Task<string> RegisterAsync(RegisterDto dto)
    {
        // Проверяем, существует ли пользователь с таким логином или email
        if (await _userRepository.ExistsByLoginOrEmailAsync(dto.Login, dto.Email))
            throw new ConflictException("Пользователь с таким логином или email уже существует");

        var newUser = new User
        {
            Login = dto.Login.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Password = PasswordHasher.Hash(dto.Password)
        };

        await _userRepository.AddUserAsync(newUser);

        return await GenereticJwtToken(newUser);
    }

    public async Task<string> LoginService(RegisterDto dto)
    {
        // Получаем пользователя по логину
        var user = await _userRepository.GetByLoginAsync(dto.Login);
        if (user is null)
            throw new BadRequestException("Неправильный логин или пароль");

        if (!PasswordHasher.Verify(dto.Password, user.Password))
            throw new BadRequestException("Неправильный логин или пароль");

        return await GenereticJwtToken(user);
    }

    public async Task<string> GenereticJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Login),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var keyStr = _config["Jwt:key"] ?? string.Empty;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"] ?? "60")),
            signingCredentials: creds
        );

        return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}