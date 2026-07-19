using HabitTrakerApi.DTO.Categories;
using HabitTrakerApi.Models.Data;

namespace HabitTrakerApi.Mappers;

public static class CategoryMapper
{
    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            HabitsCount = category.Habits?.Count ?? 0
        };
    }

    public static Category ToEntity(this CreateCategoryDto dto)
    {
        return new Category { Name = dto.Name.Trim() };
    }
}
