
using HabitTrakerApi.Common.Exeptions;
using HabitTrakerApi.DTOs.Reminders;
using HabitTrakerApi.Mappers;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Repositories.Interfaces;
using HabitTrakerApi.Services.Interfaces;

namespace HabitTrakerApi.Services;

public class ReminderService : IReminderService
{
    private readonly IReminderRepository _reminderRepository;
    private readonly IHabitRepository _habitRepository;

    public ReminderService(IReminderRepository reminderRepository, IHabitRepository habitRepository)
    {
        _reminderRepository = reminderRepository;
        _habitRepository = habitRepository;
    }

    public async Task<List<ReminderDto>> GetByHabitIdAsync(int habitId, int userId, bool isAdmin)
    {
        await GetOwnedHabitAsync(habitId, userId, isAdmin);
        var reminders = await _reminderRepository.GetByHabitIdAsync(habitId);
        return reminders.Select(r => r.ToDto()).ToList();
    }

    public async Task<ReminderDto> CreateAsync(int habitId, int userId, bool isAdmin, CreateReminderDto dto)
    {
        await GetOwnedHabitAsync(habitId, userId, isAdmin);

        var reminder = dto.ToEntity(habitId);
        await _reminderRepository.AddAsync(reminder);
        await _reminderRepository.SaveChangesAsync();

        return reminder.ToDto();
    }

    public async Task<ReminderDto> UpdateAsync(int habitId, int reminderId, int userId, bool isAdmin, UpdateReminderDto dto)
    {
        await GetOwnedHabitAsync(habitId, userId, isAdmin);

        var reminder = await _reminderRepository.GetByIdAsync(reminderId)
            ?? throw new NotFoundException($"Напоминание с Id={reminderId} не найдено");

        if (reminder.HabitId != habitId)
            throw new BadRequestException("Напоминание не относится к указанной привычке");

        reminder.ApplyUpdate(dto);
        await _reminderRepository.SaveChangesAsync();

        return reminder.ToDto();
    }

    public async Task DeleteAsync(int habitId, int reminderId, int userId, bool isAdmin)
    {
        await GetOwnedHabitAsync(habitId, userId, isAdmin);

        var reminder = await _reminderRepository.GetByIdAsync(reminderId)
            ?? throw new NotFoundException($"Напоминание с Id={reminderId} не найдено");

        if (reminder.HabitId != habitId)
            throw new BadRequestException("Напоминание не относится к указанной привычке");

        _reminderRepository.Remove(reminder);
        await _reminderRepository.SaveChangesAsync();
    }

    private async Task<Habit> GetOwnedHabitAsync(int habitId, int userId, bool isAdmin)
    {
        var habit = await _habitRepository.GetByIdAsync(habitId)
            ?? throw new NotFoundException($"Привычка с Id={habitId} не найдена");

        if (!isAdmin && habit.UserId != userId)
            throw new ForbiddenException("Эта привычка принадлежит другому пользователю");

        return habit;
    }
}
