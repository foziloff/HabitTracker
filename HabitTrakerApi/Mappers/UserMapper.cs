using HabitTrakerApi.DTO.Users;
using HabitTrakerApi.Models.Data;

namespace HabitTrakerApi.Mappers;

public static class UserMapper
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Login = user.Login,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            HabitsCount = user.Habits?.Count ?? 0
        };
    }
}
