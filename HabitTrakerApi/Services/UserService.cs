using HabitTrakerApi.Common;
using HabitTrakerApi.Common.Exeptions;
using HabitTrakerApi.DTOs.Users;
using HabitTrakerApi.Mappers;
using HabitTrakerApi.Repositories.Interfaces;
using HabitTrakerApi.Services.Interfaces;

namespace HabitTrakerApi.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("Пользователь не найден");

        return user.ToDto();
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("Пользователь не найден");

        if (!string.IsNullOrWhiteSpace(dto.Login) && dto.Login != user.Login)
        {
            if (await _userRepository.GetByLoginAsync(dto.Login) is not null)
                throw new ConflictException("Логин уже занят");
            user.Login = dto.Login.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            if (await _userRepository.GetByEmailAsync(dto.Email) is not null)
                throw new ConflictException("Email уже используется");
            user.Email = dto.Email.Trim().ToLower();
        }

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return user.ToDto();
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("Пользователь не найден");

        if (!PasswordHasher.Verify(dto.CurrentPassword, user.Password))
            throw new BadRequestException("Текущий пароль указан неверно");

        user.Password = PasswordHasher.Hash(dto.NewPassword);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => u.ToDto()).ToList();
    }

    public async Task<UserDto> ChangeRoleAsync(int targetUserId, UpdateUserRoleDto dto)
    {
        var user = await _userRepository.GetByIdAsync(targetUserId)
            ?? throw new NotFoundException("Пользователь не найден");

        user.Role = dto.Role;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return user.ToDto();
    }

    public async Task DeleteAsync(int targetUserId)
    {
        var user = await _userRepository.GetByIdAsync(targetUserId)
            ?? throw new NotFoundException("Пользователь не найден");

        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync();
    }
}
