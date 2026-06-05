using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.CreateAccessory;

public record CreateAccessoryCommand(string Name, string? Description) : IRequest<Result<AccessoryDto>>;

public class CreateAccessoryCommandHandler : IRequestHandler<CreateAccessoryCommand, Result<AccessoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccessoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AccessoryDto>> Handle(CreateAccessoryCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            return Result<AccessoryDto>.Failure("El nombre es requerido", "VALIDATION");

        var accessory = new Accessory(command.Name.Trim(), command.Description?.Trim());
        await _unitOfWork.Accessories.AddAsync(accessory);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<AccessoryDto>.Success(new AccessoryDto(accessory.Id, accessory.Name, accessory.Description, accessory.IsActive));
    }
}
