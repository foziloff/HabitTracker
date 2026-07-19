namespace HabitTrakerApi.Common.Exeptions;

public class BadRequestException : Exception
{
    public BadRequestException(string s) :base("Не верные данные для записи")
    {
            
    }
}