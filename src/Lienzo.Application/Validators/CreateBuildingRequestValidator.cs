using FluentValidation;
using Lienzo.Application.DTOs;

namespace Lienzo.Application.Validators;

public class CreateBuildingRequestValidator : AbstractValidator<CreateBuildingRequest>
{
    public CreateBuildingRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Building name is required")
            .MaximumLength(200).WithMessage("Building name must not exceed 200 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters");

        RuleFor(x => x.FloorCount)
            .GreaterThan(0).WithMessage("Floor count must be greater than zero")
            .LessThanOrEqualTo(100).WithMessage("Floor count must not exceed 100");
    }
}
