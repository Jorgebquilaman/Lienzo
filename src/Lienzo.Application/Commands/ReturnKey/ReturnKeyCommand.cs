using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.ReturnKey;

public record ReturnKeyCommand(Guid DeliveryId) : IRequest<Result<bool>>;

public class ReturnKeyCommandHandler : IRequestHandler<ReturnKeyCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ReturnKeyCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(ReturnKeyCommand command, CancellationToken ct)
    {
        var delivery = (await _unitOfWork.KeyDeliveries.GetAllAsync())
            .FirstOrDefault(d => d.Id == command.DeliveryId);

        if (delivery is null)
            return Result<bool>.Failure("Entrega no encontrada", "NOT_FOUND");

        if (delivery.ReturnedAt.HasValue)
            return Result<bool>.Failure("La llave ya fue devuelta", "VALIDATION");

        delivery.Return();
        _unitOfWork.KeyDeliveries.Update(delivery);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
