using HabitTrakerApi.Models.Enums;

namespace HabitTrakerApi.Models.Data;

public class User : EntityBase
{
    public string Login { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    /// <summary>
    /// Telegram ChatId пользователя после успешной авторизации.
    /// </summary>
    public long? ChatId { get; set; }

    /// <summary>
    /// Текущий этап авторизации в Telegram.
    /// </summary>
    public TelegramAuthState TelegramAuthState { get; set; } = TelegramAuthState.None;

    /// <summary>
    /// Временное хранение ChatId до завершения авторизации.
    /// </summary>
    public long? PendingChatId { get; set; }

    public UserRole Role { get; set; } = UserRole.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Habit> Habits { get; set; } = new List<Habit>();
}