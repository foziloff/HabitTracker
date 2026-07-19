using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Models.Enums;

namespace HabitTrakerApi.Common;

// Вся логика подсчёта стриков и процента выполнения привычки живёт здесь,
// чтобы не размазывать её по сервисам.
public static class HabitAnalyticsHelper
{
    public static int CalculateCurrentStreak(Habit habit, List<HabitLog> logs)
    {
        var doneDates = SuccessfulDatesDesc(habit, logs);
        if (doneDates.Count == 0) return 0;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return habit.Type switch
        {
            HabitType.Daily => CountConsecutive(doneDates, today, 1),
            HabitType.Weekly => CountConsecutive(doneDates, today, 7),
            HabitType.Monthly => CountConsecutiveMonths(doneDates, today),
            _ => doneDates.Count > 0 ? 1 : 0 // Disposable
        };
    }

    public static int CalculateLongestStreak(Habit habit, List<HabitLog> logs)
    {
        var doneDates = SuccessfulDatesDesc(habit, logs);
        doneDates.Reverse(); // теперь по возрастанию
        if (doneDates.Count == 0) return 0;
        if (habit.Type == HabitType.Disposable) return 1;

        int longest = 1, current = 1;

        for (int i = 1; i < doneDates.Count; i++)
        {
            bool consecutive;
            if (habit.Type == HabitType.Monthly)
            {
                var prevIdx = doneDates[i - 1].Year * 12 + doneDates[i - 1].Month;
                var curIdx = doneDates[i].Year * 12 + doneDates[i].Month;
                if (curIdx == prevIdx) continue; // тот же период, не считаем дважды
                consecutive = curIdx - prevIdx == 1;
            }
            else
            {
                var step = habit.Type == HabitType.Weekly ? 7 : 1;
                consecutive = doneDates[i].DayNumber - doneDates[i - 1].DayNumber <= step;
            }

            current = consecutive ? current + 1 : 1;
            longest = Math.Max(longest, current);
        }

        return longest;
    }

    public static double CalculateCompletionRate(Habit habit, List<HabitLog> logs)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var createdDate = DateOnly.FromDateTime(habit.CreatedAt);
        var successfulPeriods = SuccessfulDatesDesc(habit, logs).Count;

        int expectedPeriods = habit.Type switch
        {
            HabitType.Daily => Math.Max(1, today.DayNumber - createdDate.DayNumber + 1),
            HabitType.Weekly => Math.Max(1, (today.DayNumber - createdDate.DayNumber) / 7 + 1),
            HabitType.Monthly => Math.Max(1, (today.Year - createdDate.Year) * 12 + today.Month - createdDate.Month + 1),
            _ => 1
        };

        var rate = successfulPeriods / (double)expectedPeriods * 100;
        return Math.Round(Math.Min(rate, 100), 1);
    }

    private static List<DateOnly> SuccessfulDatesDesc(Habit habit, List<HabitLog> logs)
    {
        return logs
            .Where(l => l.Value >= habit.TargetCount)
            .Select(l => l.DoneDate)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();
    }

    // datesDesc отсортированы по убыванию; step — макс. разрыв в днях, чтобы считаться "подряд"
    private static int CountConsecutive(List<DateOnly> datesDesc, DateOnly today, int step)
    {
        if (today.DayNumber - datesDesc[0].DayNumber > step) return 0;

        int streak = 1;
        for (int i = 1; i < datesDesc.Count; i++)
        {
            if (datesDesc[i - 1].DayNumber - datesDesc[i].DayNumber <= step)
                streak++;
            else
                break;
        }
        return streak;
    }

    private static int CountConsecutiveMonths(List<DateOnly> datesDesc, DateOnly today)
    {
        var lastIdx = datesDesc[0].Year * 12 + datesDesc[0].Month;
        var curIdx = today.Year * 12 + today.Month;
        if (curIdx - lastIdx > 1) return 0;

        int streak = 1;
        for (int i = 1; i < datesDesc.Count; i++)
        {
            var prev = datesDesc[i - 1].Year * 12 + datesDesc[i - 1].Month;
            var cur = datesDesc[i].Year * 12 + datesDesc[i].Month;
            if (prev - cur == 1) streak++;
            else if (prev - cur == 0) continue;
            else break;
        }
        return streak;
    }
}
