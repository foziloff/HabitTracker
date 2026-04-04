using FluentValidation;
using HabitTrakerApi.Models.DTO;

namespace HabitTrakerApi.FluentValidation;
using FluentValidation;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Login).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(3);
    }
}