using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetClassroomSchedule;

public record GetClassroomScheduleQuery(Guid ClassroomId, int Days = 7) : IRequest<Result<List<ReservationDto>>>;

public class GetClassroomScheduleQueryHandler : IRequestHandler<GetClassroomScheduleQuery, Result<List<ReservationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetClassroomScheduleQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ReservationDto>>> Handle(GetClassroomScheduleQuery query, CancellationToken cancellationToken)
    {
        var classroom = await _unitOfWork.Classrooms.GetWithReservationsAsync(query.ClassroomId);
        if (classroom is null)
            return Result<List<ReservationDto>>.Failure("Classroom not found", "NOT_FOUND");

        var fromDate = DateOnly.FromDateTime(DateTime.Now);
        var toDate = fromDate.AddDays(query.Days);

        var reservations = classroom.Reservations
            .Where(r => r.Date >= fromDate && r.Date <= toDate && !r.IsDeleted)
            .OrderBy(r => r.Date)
            .ThenBy(r => r.StartTime)
            .ToList();

        var dtos = _mapper.Map<List<ReservationDto>>(reservations);

        return Result<List<ReservationDto>>.Success(dtos);
    }
}
