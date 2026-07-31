using HabitTrakerApi.Models.Data;
using MediatR;

namespace HabitTrakerApi.Analytics.Queries;

// Самый длинный стрик за всю историю привычки
public record GetLongestStreakQuery(Habit Habit) : IRequest<int>;
