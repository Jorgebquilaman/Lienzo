using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetAllClassrooms;

public record GetAllClassroomsQuery(
    Guid? BuildingId,
    string? Type,
    int? MinCapacity,
    DateTime? Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Search = null) : IRequest<Result<List<ClassroomDto>>>;

public class GetAllClassroomsQueryHandler : IRequestHandler<GetAllClassroomsQuery, Result<List<ClassroomDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllClassroomsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ClassroomDto>>> Handle(GetAllClassroomsQuery query, CancellationToken cancellationToken)
    {
        var classrooms = (await _unitOfWork.Classrooms.GetAllAsync())
            .Where(c => !c.IsDeleted)
            .AsEnumerable();

        if (query.BuildingId.HasValue)
            classrooms = classrooms.Where(c => c.BuildingId == query.BuildingId.Value);

        if (query.Type is not null && Enum.TryParse<ClassroomType>(query.Type, true, out var type))
            classrooms = classrooms.Where(c => c.Type == type);

        if (query.MinCapacity.HasValue)
            classrooms = classrooms.Where(c => c.Capacity >= query.MinCapacity.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            classrooms = classrooms.Where(c =>
                c.Name.ToLower().Contains(search));
        }

        if (query.Date.HasValue && query.StartTime.HasValue && query.EndTime.HasValue)
        {
            var dateOnly = DateOnly.FromDateTime(query.Date.Value);

            if (dateOnly.DayOfWeek == DayOfWeek.Sunday)
                return Result<List<ClassroomDto>>.Success([]);

            if (dateOnly.DayOfWeek == DayOfWeek.Saturday && (query.StartTime >= new TimeOnly(16, 0) || query.EndTime > new TimeOnly(16, 0)))
                return Result<List<ClassroomDto>>.Success([]);

            if (await _unitOfWork.Holidays.IsHolidayAsync(dateOnly))
                return Result<List<ClassroomDto>>.Success([]);

            if (await _unitOfWork.Recesos.IsRecesoAsync(dateOnly))
                return Result<List<ClassroomDto>>.Success([]);

            var timeRange = new Lienzo.Domain.ValueObjects.TimeRange(query.StartTime.Value, query.EndTime.Value);

            classrooms = classrooms.Where(c =>
            {
                var reservations = c.Reservations.Where(r =>
                    r.Date == dateOnly &&
                    r.Status != ReservationStatus.Cancelled &&
                    r.Status != ReservationStatus.Rejected);
                return !reservations.Any(r =>
                    new Lienzo.Domain.ValueObjects.TimeRange(r.StartTime, r.EndTime).OverlapsWith(timeRange));
            });
        }

        var dtos = _mapper.Map<List<ClassroomDto>>(classrooms.ToList());
        return Result<List<ClassroomDto>>.Success(dtos);
    }
}
