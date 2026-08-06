using HabitTrakerApi.DbContext;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HabitTrakerApi.Repositories;

public class HabitLogRepository : GenericRepository<HabitLog>, IGenericRepository<HabitLog>, IHabitLogRepository
{
    public HabitLogRepository(AppDbContext context) : base(context) { }

    public async Task<HabitLog?> GetByHabitAndDateAsync(int habitId, DateOnly date)
    {
        return await DbSet.FirstOrDefaultAsync(l => l.HabitId == habitId && l.DoneDate == date);
    }

    public async Task<List<HabitLog>> GetByHabitIdAsync(int habitId, DateOnly? from = null, DateOnly? to = null)
    {
        var query = DbSet.Where(l => l.HabitId == habitId);

        if (from.HasValue)
            query = query.Where(l => l.DoneDate >= from.Value);

        if (to.HasValue)
            query = query.Where(l => l.DoneDate <= to.Value);

        return await query
            .AsNoTracking()
            .OrderByDescending(l => l.DoneDate)
            .ToListAsync();
    }
}
