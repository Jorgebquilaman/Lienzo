using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Queries.GetActiveKeyDeliveries;

public record GetActiveKeyDeliveriesQuery : IRequest<Result<KeyDeliveryListResponse>>;

public class GetActiveKeyDeliveriesQueryHandler : IRequestHandler<GetActiveKeyDeliveriesQuery, Result<KeyDeliveryListResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public GetActiveKeyDeliveriesQueryHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<Result<KeyDeliveryListResponse>> Handle(GetActiveKeyDeliveriesQuery query, CancellationToken ct)
    {
        var deliveries = (await _unitOfWork.KeyDeliveries.GetAllAsync())
            .Where(d => d.ReturnedAt == null)
            .OrderByDescending(d => d.DeliveredAt)
            .ToList();

        if (deliveries.Count == 0)
            return Result<KeyDeliveryListResponse>.Success(new(new(), 0));

        var classroomIds = deliveries.Select(d => d.ClassroomId).Distinct().ToList();
        var classrooms = (await _unitOfWork.Classrooms.GetAllAsync())
            .Where(c => classroomIds.Contains(c.Id))
            .ToDictionary(c => c.Id, c => c);
        var buildingIds = classrooms.Values.Select(c => c.BuildingId).Distinct().ToList();
        var buildings = (await _unitOfWork.Buildings.GetAllAsync())
            .Where(b => buildingIds.Contains(b.Id))
            .ToDictionary(b => b.Id, b => b.Name);

        var usersResult = await _authService.GetAllUsersAsync();
        var userNames = usersResult.IsSuccess
            ? usersResult.Value.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}")
            : new Dictionary<Guid, string>();

        var localNow = DateTime.Now;
        var today = DateOnly.FromDateTime(localNow);
        var currentTime = TimeOnly.FromDateTime(localNow);

        var tomorrowReservations = await _unitOfWork.Reservations.GetByDateRangeAsync(today, today.AddDays(7));

        var items = deliveries.Select(d =>
        {
            var cls = classrooms.GetValueOrDefault(d.ClassroomId);

            // Find the first upcoming reservation for this classroom
            var nextReservation = tomorrowReservations
                .Where(r => r.ClassroomId == d.ClassroomId
                    && r.Status == ReservationStatus.Approved
                    && (r.Date > today || (r.Date == today && r.StartTime > currentTime)))
                .OrderBy(r => r.Date)
                .ThenBy(r => r.StartTime)
                .FirstOrDefault();

            NextReservationInfo? nextInfo = null;
            if (nextReservation != null)
            {
                var profName = userNames.GetValueOrDefault(nextReservation.UserId, "Desconocido");
                nextInfo = new NextReservationInfo(
                    nextReservation.Id, profName, nextReservation.UserId,
                    nextReservation.StartTime.ToString(), nextReservation.EndTime.ToString());
            }

            return new KeyDeliveryActiveDto(
                d.Id, d.ClassroomId,
                cls?.Name ?? "",
                cls is not null && buildings.TryGetValue(cls.BuildingId, out var bn) ? bn : null,
                d.DeliveredToUserId, d.DeliveredToName,
                d.DeliveredAt, d.Notes,
                nextInfo);
        }).ToList();

        return Result<KeyDeliveryListResponse>.Success(new(items.Cast<KeyDeliveryDto>().ToList(), items.Count));
    }
}
