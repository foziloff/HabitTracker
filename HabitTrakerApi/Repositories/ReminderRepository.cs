using HabitTrakerApi.DbContext;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HabitTrakerApi.Repositories;

public class ReminderRepository : BaseRepository<Reminder>, IReminderRepository
{
    public ReminderRepository(AppDbContext context) : base(context) { }

    public async Task<List<Reminder>> GetByHabitIdAsync(int habitId)
    {
        return await DbSet
            .Where(r => r.HabitId == habitId)
            .AsNoTracking()
            .OrderBy(r => r.ReminderTime)
            .ToListAsync();
    }
}
