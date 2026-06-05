using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.UpdateAccessory;

public record UpdateAccessoryCommand(Guid Id, string Name, string? Description, bool IsActive) : IRequest<Result<AccessoryDto>>;

public class UpdateAccessoryCommandHandler : IRequestHandler<UpdateAccessoryCommand, Result<AccessoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAccessoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AccessoryDto>> Handle(UpdateAccessoryCommand command, CancellationToken ct)
    {
        var accessory = await _unitOfWork.Accessories.GetByIdAsync(command.Id);
        if (accessory is null)
            return Result<AccessoryDto>.Failure("Accesorio no encontrado", "NOT_FOUND");

        accessory.Update(command.Name.Trim(), command.Description?.Trim(), command.IsActive);
        _unitOfWork.Accessories.Update(accessory);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<AccessoryDto>.Success(new AccessoryDto(accessory.Id, accessory.Name, accessory.Description, accessory.IsActive));
    }
}
