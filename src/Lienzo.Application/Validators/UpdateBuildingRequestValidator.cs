using FluentValidation;
using Lienzo.Application.DTOs;

namespace Lienzo.Application.Validators;

public class UpdateBuildingRequestValidator : AbstractValidator<UpdateBuildingRequest>
{
    public UpdateBuildingRequestValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Building name cannot be empty")
                .MaximumLength(200).WithMessage("Building name must not exceed 200 characters");
        });

        When(x => x.Address is not null, () =>
        {
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address cannot be empty")
                .MaximumLength(500).WithMessage("Address must not exceed 500 characters");
        });

        When(x => x.FloorCount is not null, () =>
        {
            RuleFor(x => x.FloorCount)
                .GreaterThan(0).WithMessage("Floor count must be greater than zero")
                .LessThanOrEqualTo(100).WithMessage("Floor count must not exceed 100");
        });
    }
}
