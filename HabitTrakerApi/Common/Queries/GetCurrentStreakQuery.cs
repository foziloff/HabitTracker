using HabitTrakerApi.Models.Data;
using MediatR;

namespace HabitTrakerApi.Analytics.Queries;

// Текущий стрик (сколько подряд периодов привычка выполняется без пропуска)
public record GetCurrentStreakQuery(Habit Habit) : IRequest<int>;
