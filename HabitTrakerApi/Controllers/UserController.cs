
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HabitTrakerApi.Controllers;

[ApiController]
[Route("api/Users") ]
public class UserController
{
    private  readonly ILogger<UserController> _logger;
    private IUserService  _userService;
    private IHabitService  _habitService;
    
    public UserController(ILogger<UserController> logger , IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpPost]
    [Route("Registration")]
    public ActionResult<string> Registration(string login, string password ,string email)
    {
        User newUser = new User()
        {
            Login = login,
            Password = password,
            Email = email
        };
        string response = _userService.Add(newUser);
       
        _logger.LogInformation(response);
        return   response ;
    }

    public string AddHabits(Habit habit, string login, string password)
    { 
        string response  =  _habitService.AddHabit(habit , login, password);
        _logger.LogInformation($"пользователь {login} добвил привычку {habit.Title}");
        return response;
    }

    public string DeleteMyHabit(string login, string password, Habit habit)
    {
       return _habitService.DeleteHabit(habit, login, password);
    }

    public List<Habit> GetMyHabits(string login, string password)
    {
        return _habitService.GetMyHabits(login, password);
    }

}