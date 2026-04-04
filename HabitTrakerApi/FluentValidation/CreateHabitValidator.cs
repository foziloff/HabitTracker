using FluentValidation;
using HabitTrakerApi.Models.DTO;

namespace HabitTrakerApi.FluentValidation;

public class CreateHabitValidator : AbstractValidator<CreateHabitDto>
{
    public CreateHabitValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
    }
}