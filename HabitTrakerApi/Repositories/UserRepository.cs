using HabitTrakerApi.DbContext;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HabitTrakerApi.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByLoginAsync(string login)
    {
        return await DbSet.Include(u => u.Habits)
            .FirstOrDefaultAsync(u => u.Login.ToLower() == login.ToLower());
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> ExistsByLoginOrEmailAsync(string login, string email)
    {
        return await DbSet.AnyAsync(u =>
            u.Login.ToLower() == login.ToLower() ||
            u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> AddUserAsync(User user)
    {

      var newUser = await Context.Users.AddAsync(user);
      await Context.SaveChangesAsync();
      return true;
    }
}
