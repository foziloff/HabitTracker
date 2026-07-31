namespace HabitTrakerApi.Analytics;

// Без изменений — тот же контракт результата, что и раньше.
public record HabitAnalyticsResult(int CurrentStreak, int LongestStreak, double CompletionRate);
