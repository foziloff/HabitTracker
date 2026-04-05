using FluentValidation;
using HabitTrakerApi.Models.Data;

namespace HabitTrakerApi.FluentValidation;

public class UserValidation :AbstractValidator<User>
{
    public UserValidation()
    {
        RuleFor(user => user.Password)
            .NotEmpty().WithMessage("пароль не может быть пустым")
            .MinimumLength(6).WithMessage("Пароль должен состоять минимун из 6 символов !");
    }
    
}