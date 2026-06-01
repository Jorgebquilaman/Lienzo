using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using Lienzo.Domain.ValueObjects;
using MediatR;

namespace Lienzo.Application.Queries.CheckAvailability;

public record CheckAvailabilityQuery(Guid ClassroomId, DateTime Date, TimeOnly StartTime, TimeOnly EndTime) : IRequest<Result<AvailabilityResponse>>;

public class CheckAvailabilityQueryHandler : IRequestHandler<CheckAvailabilityQuery, Result<AvailabilityResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CheckAvailabilityQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<AvailabilityResponse>> Handle(CheckAvailabilityQuery query, CancellationToken cancellationToken)
    {
        var classroom = await _unitOfWork.Classrooms.GetWithReservationsAsync(query.ClassroomId);
        if (classroom is null || classroom.IsDeleted)
            return Result<AvailabilityResponse>.Failure("Classroom not found", "NOT_FOUND");

        if (!classroom.IsActive)
        {
            return Result<AvailabilityResponse>.Success(new AvailabilityResponse(
                query.ClassroomId,
                query.Date,
                query.StartTime,
                query.EndTime,
                false,
                "Classroom is not active"));
        }

        var dateOnly = DateOnly.FromDateTime(query.Date);
        var timeRange = new TimeRange(query.StartTime, query.EndTime);
        var resStart = query.Date.Date + query.StartTime.ToTimeSpan();
        var resEnd = query.Date.Date + query.EndTime.ToTimeSpan();

        var maintenanceBlocks = await _unitOfWork.MaintenanceBlocks.GetAllAsync();
        var inMaintenance = maintenanceBlocks
            .Where(m => m.ClassroomId == query.ClassroomId && m.IsActive)
            .Any(m => m.StartTime.ToLocalTime() < resEnd && m.EndTime.ToLocalTime() > resStart);

        if (inMaintenance)
        {
            return Result<AvailabilityResponse>.Success(new AvailabilityResponse(
                query.ClassroomId,
                query.Date,
                query.StartTime,
                query.EndTime,
                false,
                "Classroom is under maintenance"));
        }

        var conflict = classroom.Reservations
            .Where(r => r.Date == dateOnly &&
                        r.Status != ReservationStatus.Cancelled &&
                        r.Status != ReservationStatus.Rejected)
            .Select(r => new { r.StartTime, r.EndTime })
            .FirstOrDefault(r => new TimeRange(r.StartTime, r.EndTime).OverlapsWith(timeRange));

        if (conflict is not null)
        {
            return Result<AvailabilityResponse>.Success(new AvailabilityResponse(
                query.ClassroomId,
                query.Date,
                query.StartTime,
                query.EndTime,
                false,
                $"Conflicts with reservation from {conflict.StartTime} to {conflict.EndTime}"));
        }

        return Result<AvailabilityResponse>.Success(new AvailabilityResponse(
            query.ClassroomId,
            query.Date,
            query.StartTime,
            query.EndTime,
            true,
            null));
    }
}
