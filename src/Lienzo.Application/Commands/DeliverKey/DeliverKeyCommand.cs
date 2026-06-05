using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Commands.DeliverKey;

public record DeliverKeyCommand(DeliverKeyRequest Request) : IRequest<Result<KeyDeliveryDto>>;

public class DeliverKeyCommandHandler : IRequestHandler<DeliverKeyCommand, Result<KeyDeliveryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;

    public DeliverKeyCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _authService = authService;
    }

    public async Task<Result<KeyDeliveryDto>> Handle(DeliverKeyCommand command, CancellationToken ct)
    {
        var req = command.Request;

        if (string.IsNullOrWhiteSpace(req.DeliveredToName))
            return Result<KeyDeliveryDto>.Failure("Debe especificar a quién se entrega la llave", "VALIDATION");

        var classroom = (await _unitOfWork.Classrooms.GetAllAsync())
            .FirstOrDefault(c => c.Id == req.ClassroomId);
        if (classroom is null)
            return Result<KeyDeliveryDto>.Failure("Aula no encontrada", "NOT_FOUND");

        var delivery = new KeyDelivery(
            req.ClassroomId, _currentUser.UserId,
            req.DeliveredToName, req.DeliveredToUserId, req.Notes);

        await _unitOfWork.KeyDeliveries.AddAsync(delivery);

        if (req.AccessoryIds?.Count > 0)
        {
            var validIds = await _unitOfWork.Accessories.Query()
                .Where(a => a.IsActive && req.AccessoryIds.Contains(a.Id))
                .Select(a => a.Id)
                .ToListAsync(ct);

            foreach (var accessoryId in validIds)
            {
                delivery.Accessories.Add(new KeyDeliveryAccessory(delivery.Id, accessoryId));
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var building = (await _unitOfWork.Buildings.GetAllAsync())
            .FirstOrDefault(b => b.Id == classroom.BuildingId);

        var usersResult = await _authService.GetAllUsersAsync();
        var deliveredByName = usersResult.IsSuccess
            ? usersResult.Value
                .Where(u => u.Id == _currentUser.UserId)
                .Select(u => $"{u.FirstName} {u.LastName}")
                .FirstOrDefault() ?? ""
            : "";

        var accessoryDtos = delivery.Accessories
            .Where(a => a.Accessory != null)
            .Select(a => new AccessoryDto(a.Accessory.Id, a.Accessory.Name, a.Accessory.Description, a.Accessory.IsActive))
            .ToList();

        var dto = new KeyDeliveryDto(
            delivery.Id, delivery.ClassroomId,
            classroom.Name, building?.Name,
            delivery.DeliveredToUserId, delivery.DeliveredToName,
            delivery.DeliveredById, deliveredByName,
            delivery.DeliveredAt, delivery.ReturnedAt, delivery.Notes,
            accessoryDtos.Count > 0 ? accessoryDtos : null);

        return Result<KeyDeliveryDto>.Success(dto);
    }
}
