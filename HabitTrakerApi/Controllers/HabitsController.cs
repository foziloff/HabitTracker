using HabitTrakerApi.Common;
using HabitTrakerApi.DTOs.Habits;
using HabitTrakerApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTrakerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HabitsController : ControllerBase
{
    private readonly IHabitService _habitService;
    private readonly ICurrentUserService _currentUser;

    public HabitsController(IHabitService habitService, ICurrentUserService currentUser)
    {
        _habitService = habitService;
        _currentUser = currentUser;
    }

    /// <summary>Список привычек текущего пользователя с фильтрами и пагинацией</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] HabitQueryParams query)
    {
        var result = await _habitService.GetAllAsync(_currentUser.UserId, query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _habitService.GetByIdAsync(id, _currentUser.UserId, _currentUser.IsAdmin);
        return Ok(result);
    }

    [HttpGet("{id:int}/stats")]
    public async Task<IActionResult> GetStats(int id)
    {
        var result = await _habitService.GetStatsAsync(id, _currentUser.UserId, _currentUser.IsAdmin);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHabitDto dto)
    {
        var created = await _habitService.CreateAsync(_currentUser.UserId, dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateHabitDto dto)
    {  
        var result = await _habitService.UpdateAsync(id, _currentUser.UserId, _currentUser.IsAdmin, dto);
        return Ok(result);
    }
    
    /// <summary>Смена статуса (Active/Paused/Completed/Archived)</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateHabitStatusDto dto)
    {
        var result = await _habitService.UpdateStatusAsync(id, _currentUser.UserId, _currentUser.IsAdmin, dto);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _habitService.DeleteAsync(id, _currentUser.UserId, _currentUser.IsAdmin);
        return NoContent();
    }
}
