using FluentValidation;
using Lienzo.Application.DTOs;

namespace Lienzo.Application.Validators;

public class CreateClassroomRequestValidator : AbstractValidator<CreateClassroomRequest>
{
    public CreateClassroomRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Classroom name is required")
            .MaximumLength(100).WithMessage("Classroom name must not exceed 100 characters");

        RuleFor(x => x.BuildingId)
            .NotEmpty().WithMessage("Building is required");

        RuleFor(x => x.Floor)
            .GreaterThanOrEqualTo(0).WithMessage("Floor cannot be negative");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than zero")
            .LessThanOrEqualTo(500).WithMessage("Capacity must not exceed 500");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Classroom type is required")
            .Must(t => t is "General" or "Dance" or "Drawing" or "Music" or "Lecture" or "Laboratory" or "Workshop" or "Seminar" or "Auditorium" or "Office")
            .WithMessage("Type must be a valid classroom type");
    }
}
