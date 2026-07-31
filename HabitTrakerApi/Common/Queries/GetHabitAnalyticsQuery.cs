using HabitTrakerApi.Models.Data;
using MediatR;

namespace HabitTrakerApi.Analytics.Queries;

// Агрегирующий запрос: собирает все три метрики сразу.
// Хендлер этого запроса сам является CQRS-клиентом — он не считает
// ничего напрямую, а рассылает GetCurrentStreakQuery/GetLongestStreakQuery/
// GetCompletionRateQuery через IMediator и объединяет результаты.
public record GetHabitAnalyticsQuery(Habit Habit) : IRequest<HabitAnalyticsResult>;
