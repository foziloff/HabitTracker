using HabitTrakerApi.Analytics;
using HabitTrakerApi.Analytics.Queries;
using HabitTrakerApi.Common;
using HabitTrakerApi.Common.Exeptions;
using HabitTrakerApi.DTO.Habits;
using HabitTrakerApi.Mappers;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Models.Enums;
using HabitTrakerApi.Repositories.Interfaces;
using HabitTrakerApi.Services.Interfaces;
using MediatR;

namespace HabitTrakerApi.Services;

public class HabitService : IHabitService
{
    private readonly IHabitRepository _habitRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMediator _mediator;

    public HabitService(IHabitRepository habitRepository, ICategoryRepository categoryRepository, IMediator mediator)
    {
        _habitRepository = habitRepository;
        _categoryRepository = categoryRepository;
        _mediator = mediator;
    }

    public async Task<PagedResult<HabitDto>> GetAllAsync(int userId, HabitQueryParams query)
    {
        var (items, totalCount) = await _habitRepository.GetFilteredAsync(userId, query);

        var dtos = new List<HabitDto>();
        foreach (var habit in items)
        {
            var analytics = await _mediator.Send(new GetHabitAnalyticsQuery(habit));
            dtos.Add(habit.ToDto(analytics.CurrentStreak, analytics.LongestStreak, analytics.CompletionRate));
        }

        return new PagedResult<HabitDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<HabitDto> GetByIdAsync(int habitId, int userId, bool isAdmin)
    {
        var habit = await GetOwnedHabitAsync(habitId, userId, isAdmin);

        var analytics = await _mediator.Send(new GetHabitAnalyticsQuery(habit));
        return habit.ToDto(analytics.CurrentStreak, analytics.LongestStreak, analytics.CompletionRate);
    }

    public async Task<HabitDto> CreateAsync(int userId, CreateHabitDto dto)
    {
        var categoryExists = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (categoryExists is null)
            throw new NotFoundException($"Категория с Id={dto.CategoryId} не найдена");

        var habit = dto.ToEntity(userId);

        ClearIrrelevantSchedule(habit);
        ValidateExecutionSchedule(habit);

        await _habitRepository.AddAsync(habit);
        await _habitRepository.SaveChangesAsync();

        var created = await _habitRepository.GetByIdWithDetailsAsync(habit.Id);
        return created!.ToDto();
    }

    public async Task<HabitDto> UpdateAsync(int habitId, int userId, bool isAdmin, UpdateHabitDto dto)
    {
        var habit = await GetOwnedHabitAsync(habitId, userId, isAdmin);

        if (dto.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId.Value);
            if (category is null)
                throw new NotFoundException($"Категория с Id={dto.CategoryId} не найдена");
        }

        habit.ApplyUpdate(dto);

        // Если тип сменился (например, был Weekly, стал Daily) — не оставляем
        // устаревшее значение ExecutionDayOfWeek/ExecutionDayOfMonth висеть в базе.
        ClearIrrelevantSchedule(habit);
        ValidateExecutionSchedule(habit);

        _habitRepository.Update(habit);
        await _habitRepository.SaveChangesAsync();

        var analytics = await _mediator.Send(new GetHabitAnalyticsQuery(habit));
        return habit.ToDto(analytics.CurrentStreak, analytics.LongestStreak, analytics.CompletionRate);
    }

    public async Task<HabitDto> UpdateStatusAsync(int habitId, int userId, bool isAdmin, UpdateHabitStatusDto dto)
    {
        var habit = await GetOwnedHabitAsync(habitId, userId, isAdmin);

        habit.Status = dto.Status;
        _habitRepository.Update(habit);
        await _habitRepository.SaveChangesAsync();

        return habit.ToDto();
    }

    public async Task DeleteAsync(int habitId, int userId, bool isAdmin)
    {
        var habit = await GetOwnedHabitAsync(habitId, userId, isAdmin);

        _habitRepository.Remove(habit);
        await _habitRepository.SaveChangesAsync();
    }

    public async Task<HabitStatsDto> GetStatsAsync(int habitId, int userId, bool isAdmin)
    {
        var habit = await GetOwnedHabitAsync(habitId, userId, isAdmin);
        var logs = habit.Logs.ToList();

        var analytics = await _mediator.Send(new GetHabitAnalyticsQuery(habit));

        return new HabitStatsDto
        {
            HabitId = habit.Id,
            Title = habit.Title,
            TotalLogs = logs.Count,
            TotalValue = logs.Sum(l => l.Value),
            CurrentStreak = analytics.CurrentStreak,
            LongestStreak = analytics.LongestStreak,
            CompletionRate = analytics.CompletionRate,
            LastDoneDate = logs.OrderByDescending(l => l.DoneDate).FirstOrDefault()?.DoneDate
        };
    }

    private async Task<Habit> GetOwnedHabitAsync(int habitId, int userId, bool isAdmin)
    {
        var habit = await _habitRepository.GetByIdWithDetailsAsync(habitId)
            ?? throw new NotFoundException($"Привычка с Id={habitId} не найдена");

        if (!isAdmin && habit.UserId != userId)
            throw new ForbiddenException("Эта привычка принадлежит другому пользователю");

        return habit;
    }

    // Weekly требует ExecutionDayOfWeek, Monthly требует ExecutionDayOfMonth (1-31).
    // Для Daily/Disposable оба поля не нужны.
    private static void ValidateExecutionSchedule(Habit habit)
    {
        if (habit.Type == HabitType.Weekly && habit.ExecutionDayOfWeek is null)
            throw new BadRequestException("Для еженедельной привычки нужно указать ExecutionDayOfWeek (день недели выполнения)");

        if (habit.Type == HabitType.Monthly)
        {
            if (habit.ExecutionDayOfMonth is null)
                throw new BadRequestException("Для ежемесячной привычки нужно указать ExecutionDayOfMonth (число месяца, 1-31)");

            if (habit.ExecutionDayOfMonth is < 1 or > 31)
                throw new BadRequestException("ExecutionDayOfMonth должен быть от 1 до 31");
        }
    }

    // Убирает значение поля расписания, если оно не относится к текущему Type привычки.
    private static void ClearIrrelevantSchedule(Habit habit)
    {
        if (habit.Type != HabitType.Weekly)
            habit.ExecutionDayOfWeek = null;

        if (habit.Type != HabitType.Monthly)
            habit.ExecutionDayOfMonth = null;
    }
}