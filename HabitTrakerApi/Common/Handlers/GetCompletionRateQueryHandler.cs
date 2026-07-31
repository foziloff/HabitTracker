using HabitTrakerApi.Analytics.Internal;
using HabitTrakerApi.Analytics.Queries;
using HabitTrakerApi.Models.Data;
using MediatR;

namespace HabitTrakerApi.Analytics.Handlers;

public class GetCompletionRateQueryHandler : IRequestHandler<GetCompletionRateQuery, double>
{
    public Task<double> Handle(GetCompletionRateQuery request, CancellationToken cancellationToken)
    {
        var logs = request.Habit.Logs?.ToList() ?? new List<HabitLog>();
        var result = HabitAnalyticsCalculations.CalculateCompletionRate(request.Habit, logs);
        return Task.FromResult(result);
    }
}
