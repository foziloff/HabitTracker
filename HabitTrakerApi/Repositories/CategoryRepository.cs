using HabitTrakerApi.Models.Data;
using HabitTrakerApi.DbContext;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HabitTrakerApi.Repositories;

public class CategoryRepository : GenericRepository<Category>, IGenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context) { }

    public async Task<List<Category>> GetAllWithHabitsCountAsync()
    {
        return await DbSet
            .Include(c => c.Habits)
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdWithHabitsAsync(int id)
    {
        return await DbSet
            .Include(c => c.Habits)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var query = DbSet.Where(c => c.Name.ToLower() == name.ToLower());
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> HasHabitsAsync(int categoryId)
    {
        return await Context.Habits.AnyAsync(h => h.CategoryId == categoryId);
    }
}
