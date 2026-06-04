using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetCampusStatus;

public record GetCampusStatusQuery : IRequest<Result<CampusStatusResponse>>;

public class GetCampusStatusQueryHandler : IRequestHandler<GetCampusStatusQuery, Result<CampusStatusResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public GetCampusStatusQueryHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<Result<CampusStatusResponse>> Handle(GetCampusStatusQuery query, CancellationToken ct)
    {
        var localNow = DateTime.Now;
        var today = DateOnly.FromDateTime(localNow);
        var currentTime = TimeOnly.FromDateTime(localNow);

        var buildings = (await _unitOfWork.Buildings.GetAllAsync())
            .Where(b => b.IsActive)
            .ToList();

        var classrooms = (await _unitOfWork.Classrooms.GetAllAsync())
            .Where(c => !c.IsDeleted)
            .ToList();

        var reservations = await _unitOfWork.Reservations.GetByDateRangeAsync(today, today);
        var activeReservations = reservations
            .Where(r => r.Status == ReservationStatus.Approved)
            .ToList();

        var allMaintenance = await _unitOfWork.MaintenanceBlocks.GetAllAsync();
        var activeMaintenance = allMaintenance
            .Where(m => m.IsActive && m.StartTime.ToUniversalTime() <= DateTime.UtcNow && m.EndTime.ToUniversalTime() > DateTime.UtcNow)
            .ToList();

        var userMap = new Dictionary<Guid, string>();
        var usersResult = await _authService.GetAllUsersAsync();
        if (usersResult.IsSuccess)
        {
            userMap = usersResult.Value.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}");
        }

        var buildingDtos = buildings.Select(building =>
        {
            var buildingClassrooms = classrooms
                .Where(c => c.BuildingId == building.Id)
                .ToList();

            var floors = buildingClassrooms
                .GroupBy(c => c.Floor)
                .OrderBy(g => g.Key)
                .Select(group =>
                {
                    var classroomDtos = group.Select(c =>
                    {
                        string status;
                        CampusReservationInfo? reservationInfo = null;

                        if (!c.IsActive)
                        {
                            status = "inactive";
                        }
                        else if (activeMaintenance.Any(m => m.ClassroomId == c.Id))
                        {
                            status = "maintenance";
                        }
                        else
                        {
                            var currentReservation = activeReservations
                                .Where(r => r.ClassroomId == c.Id && r.Date == today && r.StartTime <= currentTime && r.EndTime > currentTime)
                                .OrderBy(r => r.StartTime)
                                .FirstOrDefault();

                            if (currentReservation != null)
                            {
                                status = "occupied";
                                reservationInfo = new CampusReservationInfo(
                                    currentReservation.Id,
                                    currentReservation.Title,
                                    userMap.GetValueOrDefault(currentReservation.UserId, "Desconocido"),
                                    currentReservation.StartTime,
                                    currentReservation.EndTime);
                            }
                            else
                            {
                                status = "available";
                            }
                        }

                        return new CampusClassroomStatusDto(
                            c.Id, c.Name, c.Capacity, c.Type.ToString(), status, reservationInfo);
                    }).ToList();

                    return new CampusFloorDto(group.Key, classroomDtos);
                }).ToList();

            return new CampusBuildingDto(building.Id, building.Name, floors);
        }).ToList();

        return Result<CampusStatusResponse>.Success(new(buildingDtos, localNow));
    }
}
