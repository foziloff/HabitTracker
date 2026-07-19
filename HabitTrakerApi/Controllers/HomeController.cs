using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HabitTrakerApi.Controllers;
[Route("[controller]")]
public class HomeController : ControllerBase
{
    private IServiceHabits _serviceHabits;
   // private IServiceUsers _serviceUsers;

    public HomeController(IServiceHabits serviceHabits)
    {
        _serviceHabits = serviceHabits;
    }
    
    [HttpGet]
    [Route("GetAll")]
    public ActionResult<List<Habit>> GetAll()
    {
        return Ok(_serviceHabits.GetAllHabits());
    }
    [HttpGet("GetById")]
    public IActionResult GetById(int id)
    {
        return Ok(_serviceHabits.GetHabitById(id));
    }

    [HttpPost("Create)")]
    public IActionResult Create(Habit habit)
    {
        return Ok(_serviceHabits.CreateHabit(habit));
    }

    [HttpPut("Update")]
    public IActionResult Update(int id, Habit habit)
    {
        return Ok(_serviceHabits.UpdateHabit(id, habit));
    }
    [HttpDelete("Delete")]
    public IActionResult Delete(int id)
    {
        _serviceHabits.DeleteHabit(id);
        return Ok("Deleted");
    }
    [HttpPost("AddLog")]
    public IActionResult AddLog(HabitLog log)
    {
        return Ok(_serviceHabits.AddHabitLog(log));
    }
    [HttpPost("AddReminder")]
    public IActionResult AddReminder(Reminder reminder)
    {
        return Ok(_serviceHabits.AddReminder(reminder));
    }
}