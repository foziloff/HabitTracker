using HabitTrakerApi.Analytics.Internal;
using HabitTrakerApi.Analytics.Queries;
using HabitTrakerApi.Models.Data;
using MediatR;

namespace HabitTrakerApi.Analytics.Handlers;

public class GetLongestStreakQueryHandler : IRequestHandler<GetLongestStreakQuery, int>
{
    public Task<int> Handle(GetLongestStreakQuery request, CancellationToken cancellationToken)
    {
        var logs = request.Habit.Logs?.ToList() ?? new List<HabitLog>();
        var result = HabitAnalyticsCalculations.CalculateLongestStreak(request.Habit, logs);
        return Task.FromResult(result);
    }
}
