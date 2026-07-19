using HabitTrakerApi.Common;
using HabitTrakerApi.DTO.Auth;
using HabitTrakerApi.DTO.Categories;
using HabitTrakerApi.DTO.Habits;
using HabitTrakerApi.DTO.HabitLogs;
using HabitTrakerApi.DTO.Reminders;
using HabitTrakerApi.DTO.Users;
using HabitTrakerApi.Models.Enums;

namespace HabitTrakerApi.Services.Interfaces;


public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto);
    Task DeleteAsync(int id);
}

public interface IHabitService
{
    Task<PagedResult<HabitDto>> GetAllAsync(int userId, HabitQueryParams query);
    Task<HabitDto> GetByIdAsync(int habitId, int userId, bool isAdmin);
    Task<HabitDto> CreateAsync(int userId, CreateHabitDto dto);
    Task<HabitDto> UpdateAsync(int habitId, int userId, bool isAdmin, UpdateHabitDto dto);
    Task<HabitDto> UpdateStatusAsync(int habitId, int userId, bool isAdmin, UpdateHabitStatusDto dto);
    Task DeleteAsync(int habitId, int userId, bool isAdmin);
    Task<HabitStatsDto> GetStatsAsync(int habitId, int userId, bool isAdmin);
}

public interface IHabitLogService
{
    Task<List<HabitLogDto>> GetLogsAsync(int habitId, int userId, bool isAdmin, DateOnly? from, DateOnly? to);
    Task<HabitLogDto> CreateAsync(int habitId, int userId, bool isAdmin, CreateHabitLogDto dto);
    Task<HabitLogDto> UpdateAsync(int habitId, int logId, int userId, bool isAdmin, UpdateHabitLogDto dto);
    Task DeleteAsync(int habitId, int logId, int userId, bool isAdmin);
}

public interface IReminderService
{
    Task<List<ReminderDto>> GetByHabitIdAsync(int habitId, int userId, bool isAdmin);
    Task<ReminderDto> CreateAsync(int habitId, int userId, bool isAdmin, CreateReminderDto dto);
    Task<ReminderDto> UpdateAsync(int habitId, int reminderId, int userId, bool isAdmin, UpdateReminderDto dto);
    Task DeleteAsync(int habitId, int reminderId, int userId, bool isAdmin);
}

public interface IUserService
{
    Task<UserDto> GetProfileAsync(int userId);
    Task<UserDto> UpdateProfileAsync(int userId, UpdateUserDto dto);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto> ChangeRoleAsync(int targetUserId, UpdateUserRoleDto dto);
    Task DeleteAsync(int targetUserId);
}
