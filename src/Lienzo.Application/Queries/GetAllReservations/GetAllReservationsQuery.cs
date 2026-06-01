using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetAllReservations;

public record GetAllReservationsQuery(int Page = 1, int PageSize = 20, string? Status = null, string? Filter = null) : IRequest<Result<PaginatedResult<ReservationDto>>>;

public class GetAllReservationsQueryHandler : IRequestHandler<GetAllReservationsQuery, Result<PaginatedResult<ReservationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;

    public GetAllReservationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _authService = authService;
    }

    public async Task<Result<PaginatedResult<ReservationDto>>> Handle(GetAllReservationsQuery query, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Reservation> reservations;

        if (_currentUser.Role == UserRole.Admin.ToString())
        {
            reservations = await _unitOfWork.Reservations.GetAllWithDetailsAsync();
        }
        else
        {
            reservations = await _unitOfWork.Reservations.GetUserReservationsAsync(_currentUser.UserId);
        }

        var filtered = reservations.Where(r => !r.IsDeleted);

        if (!string.IsNullOrEmpty(query.Status) && Enum.TryParse<ReservationStatus>(query.Status, out var statusFilter))
            filtered = filtered.Where(r => r.Status == statusFilter);

        var today = DateOnly.FromDateTime(DateTime.Now);
        if (string.Equals(query.Filter, "upcoming", StringComparison.OrdinalIgnoreCase))
            filtered = filtered.Where(r => r.Date >= today);
        else if (string.Equals(query.Filter, "past", StringComparison.OrdinalIgnoreCase))
            filtered = filtered.Where(r => r.Date < today);

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;

        var paged = filteredList
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<ReservationDto>>(paged);

        // Populate user names
        if (_currentUser.Role == UserRole.Admin.ToString())
        {
            var usersResult = await _authService.GetAllUsersAsync();
            if (usersResult.IsSuccess)
            {
                var userMap = usersResult.Value.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}");
                foreach (var dto in dtos)
                {
                    if (userMap.TryGetValue(dto.UserId, out var name))
                        dto.UserName = name;
                }
            }
        }

        // Populate Actividad details
        await PopulateActividadDetails(dtos);

        return Result<PaginatedResult<ReservationDto>>.Success(
            PaginatedResult<ReservationDto>.Success(dtos, totalCount, query.Page, query.PageSize));
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
            dto.ActividadDocentes = string.Join(", ", act.Docentes.Select(d => userMap.GetValueOrDefault(d.DocenteId, d.DocenteId)));
        }
    }
}
