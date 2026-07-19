using HabitTrakerApi.Common;
using HabitTrakerApi.Common.Exeptions;
using HabitTrakerApi.DTOs.Categories;
using HabitTrakerApi.Mappers;
using HabitTrakerApi.Repositories.Interfaces;
using HabitTrakerApi.Services.Interfaces;

namespace HabitTrakerApi.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllWithHabitsCountAsync();
        return categories.Select(c => c.ToDto()).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdWithHabitsAsync(id)
            ?? throw new NotFoundException($"Категория с Id={id} не найдена");

        return category.ToDto();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        if (await _categoryRepository.ExistsByNameAsync(dto.Name))
            throw new ConflictException($"Категория с названием '{dto.Name}' уже существует");

        var category = dto.ToEntity();
        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        return category.ToDto();
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Категория с Id={id} не найдена");

        if (await _categoryRepository.ExistsByNameAsync(dto.Name, excludeId: id))
            throw new ConflictException($"Категория с названием '{dto.Name}' уже существует");

        category.Name = dto.Name.Trim();
        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();

        return category.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Категория с Id={id} не найдена");

        if (await _categoryRepository.HasHabitsAsync(id))
            throw new ConflictException("Нельзя удалить категорию, к которой привязаны привычки");

        _categoryRepository.Remove(category);
        await _categoryRepository.SaveChangesAsync();
    }
}
