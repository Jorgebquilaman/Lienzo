using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.TransferKey;

public record TransferKeyCommand(Guid DeliveryId, Guid NewUserId, string NewUserName) : IRequest<Result<bool>>;

public class TransferKeyCommandHandler : IRequestHandler<TransferKeyCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public TransferKeyCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(TransferKeyCommand command, CancellationToken ct)
    {
        var delivery = (await _unitOfWork.KeyDeliveries.GetAllAsync())
            .FirstOrDefault(d => d.Id == command.DeliveryId);

        if (delivery is null)
            return Result<bool>.Failure("Entrega no encontrada", "NOT_FOUND");

        if (delivery.ReturnedAt.HasValue)
            return Result<bool>.Failure("La llave ya fue devuelta", "VALIDATION");

        delivery.Transfer(_currentUser.UserId, command.NewUserName, command.NewUserId, null);
        _unitOfWork.KeyDeliveries.Update(delivery);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
