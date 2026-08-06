using HabitTrakerApi.Models.Data;
using HabitTrakerApi.DbContext;
using HabitTrakerApi.DTO.Habits;
using HabitTrakerApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HabitTrakerApi.Repositories;

public class HabitRepository : GenericRepository<Habit>, IGenericRepository<Habit>, IHabitRepository
{
    public HabitRepository(AppDbContext context) : base(context) { }

    public async Task<Habit?> GetByIdWithDetailsAsync(int id)
    {
        return await DbSet
            .Include(h => h.Category)
            .Include(h => h.Logs)
            .Include(h => h.Reminders)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<List<Habit>> GetAllByUserIdAsync(int userId)
    {
        return await DbSet
            .Include(h => h.Category)
            .Include(h => h.Logs)
            .Where(h => h.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(List<Habit> Items, int TotalCount)> GetFilteredAsync(int userId, HabitQueryParams query)
    {
        var habits = DbSet
            .Include(h => h.Category)
            .Include(h => h.Logs)
            .Where(h => h.UserId == userId)
            .AsQueryable();

        if (query.CategoryId.HasValue)
            habits = habits.Where(h => h.CategoryId == query.CategoryId.Value);

        if (query.Status.HasValue)
            habits = habits.Where(h => h.Status == query.Status.Value);

        if (query.Type.HasValue)
            habits = habits.Where(h => h.Type == query.Type.Value);

        if (query.Priority.HasValue)
            habits = habits.Where(h => h.Priority == query.Priority.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
            habits = habits.Where(h => h.Title.Contains(query.Search));

        var totalCount = await habits.CountAsync();

        var items = await habits
            .AsNoTracking()
            .OrderByDescending(h => h.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
