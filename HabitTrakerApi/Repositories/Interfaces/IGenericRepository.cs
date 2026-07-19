using HabitTrakerApi.Models;

namespace HabitTrakerApi.Repositories.Interfaces;

public interface IGenericRepository<T> where T : EntityBase
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> SaveChangesAsync();
}
