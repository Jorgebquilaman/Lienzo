using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Queries.GetClasesList;

public record GetClasesListQuery(
    DateOnly? Desde,
    DateOnly? Hasta,
    Guid? ActividadId,
    string? Estado,
    int Page,
    int PageSize) : IRequest<Result<PaginatedResult<ClaseListItemDto>>>;

public class ClaseListItemDto
{
    public Guid Id { get; init; }
    public string ActividadNombre { get; init; } = "";
    public string ClassroomName { get; init; } = "";
    public DateOnly Fecha { get; init; }
    public TimeOnly HoraInicio { get; init; }
    public TimeOnly HoraFin { get; init; }
    public string Estado { get; init; } = "";
    public int TotalAlumnos { get; init; }
    public int Presentes { get; init; }
    public string DocenteNombre { get; set; } = "";
    public DateTime CreatedAt { get; init; }
}

public class GetClasesListQueryHandler : IRequestHandler<GetClasesListQuery, Result<PaginatedResult<ClaseListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public GetClasesListQueryHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<Result<PaginatedResult<ClaseListItemDto>>> Handle(GetClasesListQuery request, CancellationToken ct)
    {
        var query = _unitOfWork.Clases.Query()
            .Include(c => c.Classroom)
            .Include(c => c.Actividad)
            .Include(c => c.Asistencias)
            .Where(c => !c.IsDeleted);

        if (request.Desde.HasValue)
            query = query.Where(c => c.Fecha >= request.Desde.Value);

        if (request.Hasta.HasValue)
            query = query.Where(c => c.Fecha <= request.Hasta.Value);

        if (request.ActividadId.HasValue)
            query = query.Where(c => c.ActividadId == request.ActividadId.Value);

        if (!string.IsNullOrWhiteSpace(request.Estado) && Enum.TryParse<ClaseEstado>(request.Estado, ignoreCase: true, out var estado))
            query = query.Where(c => c.Estado == estado);

        var usersResult = await _authService.GetAllUsersAsync();
        var userNames = usersResult.IsSuccess
            ? usersResult.Value.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}")
            : new Dictionary<Guid, string>();

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.Fecha)
            .ThenByDescending(c => c.HoraInicio)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ClaseListItemDto
            {
                Id = c.Id,
                ActividadNombre = c.Actividad != null ? c.Actividad.Nombre : "",
                ClassroomName = c.Classroom != null ? c.Classroom.Name : "",
                Fecha = c.Fecha,
                HoraInicio = c.HoraInicio,
                HoraFin = c.HoraFin,
                Estado = c.Estado.ToString(),
                TotalAlumnos = c.Asistencias.Count,
                Presentes = c.Asistencias.Count(a => a.Presente),
                DocenteNombre = c.CheckedInByUserId.ToString(),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(ct);

        foreach (var item in items)
        {
            if (Guid.TryParse(item.DocenteNombre, out var userId) && userNames.TryGetValue(userId, out var name))
                item.DocenteNombre = name;
        }

        return Result<PaginatedResult<ClaseListItemDto>>.Success(
            PaginatedResult<ClaseListItemDto>.Success(items, totalCount, request.Page, request.PageSize));
    }
}
