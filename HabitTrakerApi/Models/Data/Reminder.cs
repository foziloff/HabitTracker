    namespace HabitTrakerApi.Models.Data;

    public class Reminder : EntityBase
    {

        public int HabitId { get; set; }

        public TimeOnly ReminderTime { get; set; }

        public bool IsEnabled { get; set; }

        public Habit Habit { get; set; } = null!;
        
    }