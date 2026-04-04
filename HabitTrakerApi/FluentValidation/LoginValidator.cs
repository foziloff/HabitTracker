using FluentValidation;
using HabitTrakerApi.Models.DTO;

namespace HabitTrakerApi.FluentValidation;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Login).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}