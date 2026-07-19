namespace HabitTrakerApi.Common.Exeptions;

public class NotFoundException : Exception
{
    public NotFoundException(string s):base("не найдены данные")
    {
            
    }
}