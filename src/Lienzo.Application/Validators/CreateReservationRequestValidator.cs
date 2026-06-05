using FluentValidation;
using Lienzo.Application.DTOs;

namespace Lienzo.Application.Validators;

public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.ClassroomId)
            .NotEmpty().WithMessage("Classroom is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .Must(d => d >= DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("Date cannot be in the past");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required")
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time");

        RuleFor(x => x.EndDate)
            .Must((request, endDate) => endDate >= request.Date)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be on or after start date");

        RuleFor(x => x.DaysOfWeek)
            .Must(BeValidDaysOfWeek)
            .When(x => !string.IsNullOrWhiteSpace(x.DaysOfWeek))
            .WithMessage("Days of week must be comma-separated valid day names (e.g. Monday,Wednesday,Friday)");
    }

    private static bool BeValidDaysOfWeek(string? days)
    {
        if (string.IsNullOrWhiteSpace(days)) return true;
        var validDays = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
        };
        return days.Split(',').All(d => validDays.Contains(d.Trim()));
    }
}
