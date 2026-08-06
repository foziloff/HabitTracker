using HabitTrakerApi.Common;
using HabitTrakerApi.DTO.Users;
using HabitTrakerApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTrakerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUser;

    public UsersController(IUserService userService, ICurrentUserService currentUser)
    {
        _userService = userService;
        _currentUser = currentUser;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        return Ok(await _userService.GetProfileAsync(_currentUser.UserId));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserDto dto)
    {
        return Ok(await _userService.UpdateProfileAsync(_currentUser.UserId, dto));
    }

    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        await _userService.ChangePasswordAsync(_currentUser.UserId, dto);
        return NoContent();
    }

    // ПРИВЯЗКА Telegram теперь идёт только через самого бота (/start -> кнопка ->
    // логин -> пароль, см. Integrations/Telegram/TelegramUpdateProcessor.cs) —
    // отдельного эндпоинта для этого больше нет.
    // Отвязку оставляем через API — это законное действие со стороны самого приложения,
    // не требующее диалога в Telegram.

    /// <summary>Отвязать Telegram — уведомления снова будут приходить только в приложение</summary>

    /// <summary>Список всех пользователей — только Admin</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _userService.GetAllAsync());
    }

    [HttpPatch("{id:int}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] UpdateUserRoleDto dto)
    {
        return Ok(await _userService.ChangeRoleAsync(id, dto));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
}
