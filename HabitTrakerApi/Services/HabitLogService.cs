using HabitTrakerApi.Common.Exeptions;
using HabitTrakerApi.DTO.HabitLogs;
using HabitTrakerApi.Mappers;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Models.Enums;
using HabitTrakerApi.Repositories.Interfaces;
using HabitTrakerApi.Services.Interfaces;

namespace HabitTrakerApi.Services;

public class HabitLogService : IHabitLogService
{
    private readonly IHabitLogRepository _logRepository;
    private readonly IHabitRepository _habitRepository;

    public HabitLogService(IHabitLogRepository logRepository, IHabitRepository habitRepository)
    {
        _logRepository = logRepository;
        _habitRepository = habitRepository;
    }

    public async Task<List<HabitLogDto>> GetLogsAsync(int habitId, int userId, bool isAdmin, DateOnly? from, DateOnly? to)
    {
        await GetOwnedHabitAsync(habitId, userId, isAdmin);

        var logs = await _logRepository.GetByHabitIdAsync(habitId, from, to);
        return logs.Select(l => l.ToDto()).ToList();
    }

    public async Task<HabitLogDto> CreateAsync(int habitId, int userId, bool isAdmin, CreateHabitLogDto dto)
    {
        var habit = await GetOwnedHabitAsync(habitId, userId, isAdmin);

        var existing = await _logRepository.GetByHabitAndDateAsync(habitId, dto.DoneDate);
        if (existing is not null)
            throw new ConflictException($"Запись на дату {dto.DoneDate:yyyy-MM-dd} уже существует. Используйте обновление записи.");

        var log = dto.ToEntity(habitId);
        await _logRepository.AddAsync(log);

        // Для разовых привычек: как только цель достигнута - считаем привычку выполненной
        if (habit.Type == HabitType.Disposable && dto.Value >= habit.TargetCount && habit.Status == HabitStatus.Active)
        {
            habit.Status = HabitStatus.Completed;
            _habitRepository.Update(habit);
        }

        await _logRepository.SaveChangesAsync();
        return log.ToDto();
    }

    public async Task<HabitLogDto> UpdateAsync(int habitId, int logId, int userId, bool isAdmin, UpdateHabitLogDto dto)
    {
        await GetOwnedHabitAsync(habitId, userId, isAdmin);

        var log = await _logRepository.GetByIdAsync(logId)
            ?? throw new NotFoundException($"Запись с Id={logId} не найдена");

        if (log.HabitId != habitId)
            throw new BadRequestException("Запись не относится к указанной привычке");

        log.ApplyUpdate(dto);
        _logRepository.Update(log);
        await _logRepository.SaveChangesAsync();

        return log.ToDto();
    }

    public async Task DeleteAsync(int habitId, int logId, int userId, bool isAdmin)
    {
        await GetOwnedHabitAsync(habitId, userId, isAdmin);

        var log = await _logRepository.GetByIdAsync(logId)
            ?? throw new NotFoundException($"Запись с Id={logId} не найдена");

        if (log.HabitId != habitId)
            throw new BadRequestException("Запись не относится к указанной привычке");

        _logRepository.Remove(log);
        await _logRepository.SaveChangesAsync();
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
