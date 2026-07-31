using HabitTrakerApi.Models.Data;
using MediatR;

namespace HabitTrakerApi.Analytics.Queries;

// Процент выполнения от даты создания привычки до сегодня
public record GetCompletionRateQuery(Habit Habit) : IRequest<double>;
