using FluentValidation;
using Lienzo.Application.DTOs;

namespace Lienzo.Application.Validators;

public class UpdateClassroomRequestValidator : AbstractValidator<UpdateClassroomRequest>
{
    public UpdateClassroomRequestValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Classroom name cannot be empty")
                .MaximumLength(100).WithMessage("Classroom name must not exceed 100 characters");
        });

        When(x => x.Floor is not null, () =>
        {
            RuleFor(x => x.Floor)
                .GreaterThanOrEqualTo(0).WithMessage("Floor cannot be negative");
        });

        When(x => x.Capacity is not null, () =>
        {
            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Capacity must be greater than zero")
                .LessThanOrEqualTo(500).WithMessage("Capacity must not exceed 500");
        });

        When(x => x.Type is not null, () =>
        {
            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Classroom type cannot be empty")
                .Must(t => t is "General" or "Dance" or "Drawing" or "Music")
                .WithMessage("Type must be General, Dance, Drawing, or Music");
        });
    }
}
