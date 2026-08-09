using HabitTrakerApi.Common;
using HabitTrakerApi.DTO.HabitLogs;
using HabitTrakerApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTrakerApi.Controllers;

// Вложенный ресурс: /api/habits/{habitId}/logs
[ApiController]
[Route("api/habits/{habitId:int}/logs")]
[Authorize]
public class HabitLogsController : ControllerBase
{
    private readonly IHabitLogService _logService;
    private readonly ICurrentUserService _currentUser;

    public HabitLogsController(IHabitLogService logService, ICurrentUserService currentUser)
    {
        _logService = logService;
        _currentUser = currentUser;
    }

    /// <summary>История отметок по привычке, можно фильтровать по датам ?from=&amp;to=</summary>
    [HttpGet]
    public async Task<IActionResult> GetLogs(int habitId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        var result = await _logService.GetLogsAsync(habitId, _currentUser.UserId, _currentUser.IsAdmin, from, to);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int habitId, [FromBody] CreateHabitLogDto dto)
    {
        var result = await _logService.CreateAsync(habitId, _currentUser.UserId, _currentUser.IsAdmin, dto);
        return CreatedAtAction(nameof(GetLogs), new { habitId }, result);
    }

    [HttpPut("{logId:int}")]
    public async Task<IActionResult> Update(int habitId, int logId, [FromBody] UpdateHabitLogDto dto)
    {
        var result = await _logService.UpdateAsync(habitId, logId, _currentUser.UserId, _currentUser.IsAdmin, dto);
        return Ok(result);
    }

    [HttpDelete("{logId:int}")]
    public async Task<IActionResult> Delete(int habitId, int logId)
    {
        await _logService.DeleteAsync(habitId, logId, _currentUser.UserId, _currentUser.IsAdmin);
        return NoContent();
    }
}
