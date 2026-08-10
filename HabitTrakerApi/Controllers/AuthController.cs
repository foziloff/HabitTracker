using HabitTrakerApi.DTO.Auth;
using HabitTrakerApi.Services;
using HabitTrakerApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HabitTrakerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Регистрация нового пользователя</summary>
    [HttpPost("register")]
    public async Task<ActionResult<string>> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }

    /// <summary>Вход по логину и паролю, возвращает JWT</summary>
    [HttpPost("login")]
    public async Task<ActionResult<string>> Login([FromBody] RegisterDto dto)
    {
        var result
            = await _authService.LoginService(dto);
        
        return Ok(result);
    }
}
