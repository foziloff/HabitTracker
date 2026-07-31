using HabitTrakerApi.Analytics;
using HabitTrakerApi.Analytics.Queries;
using HabitTrakerApi.Common;
using HabitTrakerApi.Common.Exeptions;
using HabitTrakerApi.DTO.Habits;
using HabitTrakerApi.Mappers;
using HabitTrakerApi.Models.Data;
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
}
