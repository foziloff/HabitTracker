using HabitTrakerApi.Common;
using HabitTrakerApi.DTO.Reminders;
using HabitTrakerApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTrakerApi.Controllers;

// Вложенный ресурс: /api/habits/{habitId}/reminders
[ApiController]
[Route("api/habits/{habitId:int}/reminders")]
[Authorize]
public class RemindersController : ControllerBase
{
    private readonly IReminderService _reminderService;
    private readonly ICurrentUserService _currentUser;

    public RemindersController(IReminderService reminderService, ICurrentUserService currentUser)
    {
        _reminderService = reminderService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int habitId)
    {
        var result = await _reminderService.GetByHabitIdAsync(habitId, _currentUser.UserId, _currentUser.IsAdmin);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int habitId, [FromBody] CreateReminderDto dto)
    {
        var result = await _reminderService.CreateAsync(habitId, _currentUser.UserId, _currentUser.IsAdmin, dto);
        return CreatedAtAction(nameof(GetAll), new { habitId }, result);
    }

    [HttpPut("{reminderId:int}")]
    public async Task<IActionResult> Update(int habitId, int reminderId, [FromBody] UpdateReminderDto dto)
    {
        var result = await _reminderService.UpdateAsync(habitId, reminderId, _currentUser.UserId, _currentUser.IsAdmin, dto);
        return Ok(result);
    }

    [HttpDelete("{reminderId:int}")]
    public async Task<IActionResult> Delete(int habitId, int reminderId)
    {
        await _reminderService.DeleteAsync(habitId, reminderId, _currentUser.UserId, _currentUser.IsAdmin);
        return NoContent();
    }
}
