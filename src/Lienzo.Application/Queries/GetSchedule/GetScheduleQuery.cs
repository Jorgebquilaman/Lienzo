using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetSchedule;

public record GetScheduleQuery(DateTime FromDate, DateTime ToDate, Guid? BuildingId = null, Guid? ClassroomId = null) : IRequest<Result<List<ReservationDto>>>;

public class GetScheduleQueryHandler : IRequestHandler<GetScheduleQuery, Result<List<ReservationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;

    public GetScheduleQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _authService = authService;
    }

    public async Task<Result<List<ReservationDto>>> Handle(GetScheduleQuery query, CancellationToken cancellationToken)
    {
        var fromDateOnly = DateOnly.FromDateTime(query.FromDate);
        var toDateOnly = DateOnly.FromDateTime(query.ToDate);

        var reservations = await _unitOfWork.Reservations.GetByDateRangeAsync(
            fromDateOnly, toDateOnly, query.BuildingId, query.ClassroomId);

        var dtos = _mapper.Map<List<ReservationDto>>(reservations);

        await PopulateUserNames(dtos);
        await PopulateActividadDetails(dtos);

        return Result<List<ReservationDto>>.Success(dtos);
    }

    private async Task PopulateUserNames(List<ReservationDto> dtos)
    {
        var usersResult = await _authService.GetAllUsersAsync();
        if (!usersResult.IsSuccess) return;
        var userMap = usersResult.Value.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}");
        foreach (var dto in dtos)
        {
            if (userMap.TryGetValue(dto.UserId, out var name))
                dto.UserName = name;
        }
    }

    private async Task PopulateActividadDetails(List<ReservationDto> dtos)
    {
        var ids = dtos.Where(d => d.ActividadId.HasValue).Select(d => d.ActividadId!.Value).Distinct().ToList();
        if (ids.Count == 0) return;

        var actividades = await _unitOfWork.Actividades.GetAllWithDetailsAsync();
        var actividadesMap = actividades.Where(a => ids.Contains(a.Id)).ToDictionary(a => a.Id);

        var usersResult = await _authService.GetAllUsersAsync();
        var userMap = usersResult.IsSuccess
            ? usersResult.Value.ToDictionary(u => u.Id.ToString(), u => $"{u.FirstName} {u.LastName}")
            : new Dictionary<string, string>();

        foreach (var dto in dtos)
        {
            if (!dto.ActividadId.HasValue || !actividadesMap.TryGetValue(dto.ActividadId.Value, out var act)) continue;
            dto.ActividadNombre = act.Nombre;
            dto.ActividadPeriodo = act.Periodo?.Nombre;
            dto.ActividadCarrera = act.Carrera?.Nombre;
            dto.ActividadDocentes = string.Join(", ", act.Docentes.Select(d => userMap.GetValueOrDefault(d.DocenteId, d.DocenteId)).Distinct());
        }
    }
}
