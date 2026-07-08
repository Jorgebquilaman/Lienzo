using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Queries.GetMisPendientes;

public record GetMisPendientesQuery : IRequest<Result<List<MisPendientesDto>>>;

public class MisPendientesDto
{
    public Guid ClaseId { get; init; }
    public string ActividadNombre { get; init; } = "";
    public string ClassroomName { get; init; } = "";
    public DateOnly Fecha { get; init; }
    public TimeOnly HoraInicio { get; init; }
    public TimeOnly HoraFin { get; init; }
    public string DocenteNombre { get; set; } = "";
}

public class GetMisPendientesQueryHandler : IRequestHandler<GetMisPendientesQuery, Result<List<MisPendientesDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;

    public GetMisPendientesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _authService = authService;
    }

    public async Task<Result<List<MisPendientesDto>>> Handle(GetMisPendientesQuery request, CancellationToken ct)
    {
        var sgaPersonaId = await _authService.GetSgaPersonaIdAsync(_currentUser.UserId);
        if (sgaPersonaId is null)
            return Result<List<MisPendientesDto>>.Success([]);

        var usersResult = await _authService.GetAllUsersAsync();
        var userNames = usersResult.IsSuccess
            ? usersResult.Value.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}")
            : new Dictionary<Guid, string>();

        var pendientes = await _unitOfWork.AsistenciasAlumnos.Query()
            .Include(a => a.Clase).ThenInclude(c => c.Actividad)
            .Include(a => a.Clase).ThenInclude(c => c.Classroom)
            .Where(a => a.SgaPersonaId == sgaPersonaId.Value
                     && !a.Presente
                     && a.Clase.Estado == ClaseEstado.Abierta
                     && !a.Clase.IsDeleted)
            .Select(a => new MisPendientesDto
            {
                ClaseId = a.ClaseId,
                ActividadNombre = a.Clase.Actividad != null ? a.Clase.Actividad.Nombre : "",
                ClassroomName = a.Clase.Classroom != null ? a.Clase.Classroom.Name : "",
                Fecha = a.Clase.Fecha,
                HoraInicio = a.Clase.HoraInicio,
                HoraFin = a.Clase.HoraFin,
                DocenteNombre = a.Clase.CheckedInByUserId.ToString()
            })
            .ToListAsync(ct);

        foreach (var item in pendientes)
        {
            if (Guid.TryParse(item.DocenteNombre, out var userId) && userNames.TryGetValue(userId, out var name))
                item.DocenteNombre = name;
        }

        return Result<List<MisPendientesDto>>.Success(pendientes);
    }
}
