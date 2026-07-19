using System.Security.Claims;
using HabitTrakerApi.Models.Enums;

namespace HabitTrakerApi.Common;

public interface ICurrentUserService
{
    int UserId { get; }
    UserRole Role { get; }
    bool IsAdmin { get; }
    bool IsAuthenticated { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public bool IsAuthenticated => _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public int UserId
    {
        get
        {
            var claim = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }

    public UserRole Role
    {
        get
        {
            var claim = _accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(claim, out var role) ? role : UserRole.User;
        }
    }

    public bool IsAdmin => Role == UserRole.Admin;
}
