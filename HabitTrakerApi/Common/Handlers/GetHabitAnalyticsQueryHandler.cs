using HabitTrakerApi.Analytics.Queries;
using MediatR;
namespace HabitTrakerApi.Analytics.Handlers;

// Композитный хендлер: не считает ничего сам, а раскладывает задачу
// на три независимых CQRS-запроса и параллельно их выполняет через IMediator.
public class GetHabitAnalyticsQueryHandler : IRequestHandler<GetHabitAnalyticsQuery, HabitAnalyticsResult>
{
    private readonly IMediator _mediator;

    public GetHabitAnalyticsQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<HabitAnalyticsResult> Handle(GetHabitAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var currentStreakTask = _mediator.Send(new GetCurrentStreakQuery(request.Habit), cancellationToken);
        var longestStreakTask = _mediator.Send(new GetLongestStreakQuery(request.Habit), cancellationToken);
        var completionRateTask = _mediator.Send(new GetCompletionRateQuery(request.Habit), cancellationToken);

        await Task.WhenAll(currentStreakTask, longestStreakTask, completionRateTask);

        return new HabitAnalyticsResult(
            CurrentStreak: currentStreakTask.Result,
            LongestStreak: longestStreakTask.Result,
            CompletionRate: completionRateTask.Result);
    }
}
