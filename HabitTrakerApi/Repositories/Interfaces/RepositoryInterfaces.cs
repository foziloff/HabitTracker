using HabitTrakerApi.DTO.Habits;
using HabitTrakerApi.Models.Data;

namespace HabitTrakerApi.Repositories.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<List<Category>> GetAllWithHabitsCountAsync();
    Task<Category?> GetByIdWithHabitsAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> HasHabitsAsync(int categoryId);
}

public interface IHabitRepository : IGenericRepository<Habit>
{
    Task<Habit?> GetByIdWithDetailsAsync(int id);
    Task<(List<Habit> Items, int TotalCount)> GetFilteredAsync(int userId, HabitQueryParams query);
    Task<List<Habit>> GetAllByUserIdAsync(int userId);
}

public interface IHabitLogRepository : IGenericRepository<HabitLog>
{
    Task<HabitLog?> GetByHabitAndDateAsync(int habitId, DateOnly date);
    Task<List<HabitLog>> GetByHabitIdAsync(int habitId, DateOnly? from = null, DateOnly? to = null);
}

public interface IReminderRepository : IGenericRepository<Reminder>
{
    Task<List<Reminder>> GetByHabitIdAsync(int habitId);
}

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByLoginAsync(string login);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsByLoginOrEmailAsync(string login, string email);
    Task<bool> AddUserAsync(User user);
}
