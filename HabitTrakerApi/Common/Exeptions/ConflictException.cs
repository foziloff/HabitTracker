namespace HabitTrakerApi.Common.Exeptions;

public class ConflictException : Exception
{
    public ConflictException(string s) : base("ошибка записи выполнение привычки!")
    {
        
    }

}