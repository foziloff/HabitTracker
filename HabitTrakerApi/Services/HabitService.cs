using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Models.DTO;
using AutoMapper;
using HabitTrakerApi.Repositories;


namespace HabitTrakerApi.Services;

public interface IHabitService
{
    void Create(CreateHabitDto dto, string token);
    void Complete(int habitId, string token);
    List<HabitDto> GetMy(string token);
}

public class HabitService : IHabitService
{
    private readonly IHabitRepository _repo;
    private readonly IAuthService _auth;
    private readonly IMapper _mapper;

    public HabitService(IHabitRepository repo, IAuthService auth, IMapper mapper)
    {
        _repo = repo;
        _auth = auth;
        _mapper = mapper;
    }

    public void Create(CreateHabitDto dto, string token)
    {
        var user = _auth.GetCurrent(token);

        var habit = new Habit
        {
            Title = dto.Title,
            Type = dto.Type,
            UserId = user.id
        };

        _repo.Add(habit);
    }

    public void Complete(int habitId, string token)
    {
        var user = _auth.GetCurrent(token);

        var habit = _repo.GetById(habitId);

        if (habit == null || habit.UserId != user.id)
            throw new Exception("Access denied");

        habit.IsCompleted = true;
    }

    public List<HabitDto> GetMy(string token)
    {
        var user = _auth.GetCurrent(token);
        return _mapper.Map<List<HabitDto>>(_repo.GetByUserId(user.id));
    }
}
