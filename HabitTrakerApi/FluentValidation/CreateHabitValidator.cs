using FluentValidation;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Models.DTO;

namespace HabitTrakerApi.FluentValidation;

public class HabitValidator : AbstractValidator<Habit>
{
    public HabitValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название не может быть пустым")
            .Length(3, 150).WithMessage("Длина должна быть от 3 до 150 символов");
    }
}