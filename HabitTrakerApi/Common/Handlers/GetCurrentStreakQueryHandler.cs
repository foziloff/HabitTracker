using HabitTrakerApi.Analytics.Internal;
using HabitTrakerApi.Analytics.Queries;
using HabitTrakerApi.Models.Data;
using MediatR;

namespace HabitTrakerApi.Analytics.Handlers;

public class GetCurrentStreakQueryHandler : IRequestHandler<GetCurrentStreakQuery, int>
{
    public Task<int> Handle(GetCurrentStreakQuery request, CancellationToken cancellationToken)
    {
        var logs = request.Habit.Logs?.ToList() ?? new List<HabitLog>();
        var result = HabitAnalyticsCalculations.CalculateCurrentStreak(request.Habit, logs);
        return Task.FromResult(result);
    }
}
