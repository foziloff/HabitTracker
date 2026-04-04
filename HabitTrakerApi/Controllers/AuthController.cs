using HabitTrakerApi.Models.DTO;
using HabitTrakerApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HabitTrakerApi.Controllers;
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    public IActionResult Register(RegisterDto dto)
        => Ok(_service.Register(dto));

    [HttpPost("login")]
    public IActionResult Login(LoginDto dto)
        => Ok(_service.Login(dto));
}