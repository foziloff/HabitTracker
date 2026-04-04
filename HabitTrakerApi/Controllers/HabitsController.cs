using HabitTrakerApi.Models.DTO;
using HabitTrakerApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HabitTrakerApi.Controllers;

[ApiController]
[Route("api/habits")]
public class HabitsController : ControllerBase
{
    private readonly IHabitService _service;

    public HabitsController(IHabitService service)
    {
        _service = service;
    }

    [HttpPost("create")]
    public IActionResult Create(CreateHabitDto dto)
    {
        _service.Create(dto, GetToken());
        return Ok();
    }

    [HttpPost("complete")]
    public IActionResult Complete(CompleteHabitDto dto)
    {
        _service.Complete(dto.HabitId, GetToken());
        return Ok();
    }

    [HttpGet("my")]
    public IActionResult My()
        => Ok(_service.GetMy(GetToken()));

    private string GetToken()
    {
        return Request.Headers["Authorization"]
            .ToString()
            .Replace("Bearer ", "");
    }
}