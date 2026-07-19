using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HabitTrakerApi.DbContext;
using HabitTrakerApi.DTOs.Auth;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Models.DTO;
using HabitTrakerApi.Repositories;
using HabitTrakerApi.Repositories.Interfaces;
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
    
    public Task<string> RegisterAsync(RegisterDto dto)
    {
        if (JwtRepository.CheckUser(dto) is null)
        {
            return null;
        }

        User? newUser = new User() { Login = dto.Login, Email = dto.Email, Password = dto.Password };
        _userRepository.AddUserAsync(newUser);
        return GenereticJwtToken(newUser);
    }

    public Task<string> LoginService(RegisterDto dto)
    {

        var user =  JwtRepository.CheckUser(dto);
        if (user is null )
        {
            return null;
        }

        return GenereticJwtToken(user);
    }

    public async Task<string> GenereticJwtToken(User user)
    {
        var claims = new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Login),
            new(ClaimTypes.Role, user.Role.ToString())
        };
        var Key = new SymmetricSecurityKey(Encoding.UTF8 .GetBytes(_config["Jwt:key"]!));  

        var Creds = new SymmetricKeyWrapProvider(Key, SecurityAlgorithms.HmacSha256);
        var Token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"]!))
            );
        return new  JwtSecurityTokenHandler().WriteToken(Token);
    }
}