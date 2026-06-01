using FluentValidation;
using Lienzo.Application.DTOs;

namespace Lienzo.Application.Validators;

public class CreateAnnouncementRequestValidator : AbstractValidator<CreateAnnouncementRequest>
{
    public CreateAnnouncementRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required")
            .MaximumLength(5000).WithMessage("Body must not exceed 5000 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .Must(t => t is "Cancellation" or "Postponement" or "General" or "Emergency")
            .WithMessage("Type must be Cancellation, Postponement, General, or Emergency");

        RuleFor(x => x.TargetAudience)
            .NotEmpty().WithMessage("Target audience is required")
            .Must(t => t is "All" or "AllStudents" or "AllTeachers" or "SpecificClassroom" or "SpecificStudents")
            .WithMessage("Target audience must be All, AllStudents, AllTeachers, SpecificClassroom, or SpecificStudents");
    }
}
