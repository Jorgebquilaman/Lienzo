using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.SyncActividades;

public record SyncActividadesCommand(short AnioAcademico) : IRequest<Result<SyncActividadesResult>>;

public record SyncActividadesResult(int Creados, int Existentes, int SinPeriodo, int SinCarrera, int TotalExterno, int DocentesAsignados);

public class SyncActividadesCommandHandler : IRequestHandler<SyncActividadesCommand, Result<SyncActividadesResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISyncActividadService _syncService;
    private readonly IAuthService _authService;

    public SyncActividadesCommandHandler(IUnitOfWork unitOfWork, ISyncActividadService syncService, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _syncService = syncService;
        _authService = authService;
    }

    public async Task<Result<SyncActividadesResult>> Handle(SyncActividadesCommand command, CancellationToken cancellationToken)
    {
        var external = await _syncService.GetActividadesAsync(command.AnioAcademico);
        if (external.Count == 0)
            return Result<SyncActividadesResult>.Success(new SyncActividadesResult(0, 0, 0, 0, 0, 0));

        var periodos = (await _unitOfWork.Periodos.GetAllAsync()).ToList();
        var periodosByCodigo = periodos.Where(p => p.CodigoExterno.HasValue)
            .ToDictionary(p => p.CodigoExterno!.Value);

        var carreras = (await _unitOfWork.Carreras.GetAllAsync()).ToList();
        var carrerasByCodigo = carreras.Where(c => c.CodigoExterno.HasValue)
            .ToDictionary(c => c.CodigoExterno!.Value);
        var defaultCarrera = carreras.FirstOrDefault();
        if (defaultCarrera is null)
        {
            defaultCarrera = new Carrera("Sin asignar", "S/A");
            await _unitOfWork.Carreras.AddAsync(defaultCarrera);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            carreras.Add(defaultCarrera);
        }

        var existing = (await _unitOfWork.Actividades.GetAllAsync()).ToList();
        var existingByCodigo = existing.Where(a => a.CodigoExterno.HasValue && !a.IsDeleted)
            .ToDictionary(a => a.CodigoExterno!.Value);

        var usersResult = await _authService.GetAllUsersAsync();
        var users = usersResult.Value ?? [];
        var nameToUserId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        foreach (var u in users)
        {
            var fullName = $"{u.FirstName} {u.LastName}".Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(fullName) && !nameToUserId.ContainsKey(fullName))
                nameToUserId[fullName] = u.Id;
        }

        var created = 0;
        var existed = 0;
        var noPeriodo = 0;
        var noCarrera = 0;
        var docentesAsignados = 0;

        foreach (var ext in external)
        {
            Actividad? actividad;

            if (existingByCodigo.TryGetValue(ext.Comision, out var existingAct))
            {
                actividad = existingAct;
                existed++;
            }
            else
            {
                // Restore soft-deleted actividad if found
                var deleted = existing.FirstOrDefault(a => a.CodigoExterno == ext.Comision && a.IsDeleted);
                if (deleted is not null)
                {
                    deleted.IsDeleted = false;
                    deleted.DeletedAt = null;
                    existingByCodigo[ext.Comision] = deleted;
                    actividad = deleted;
                    existed++;
                }
                else
                {
                    if (!periodosByCodigo.TryGetValue(ext.PeriodoId, out var periodo))
                    {
                        noPeriodo++;
                        continue;
                    }

                    var carrera = ext.PropuestaId.HasValue && carrerasByCodigo.TryGetValue(ext.PropuestaId.Value, out var match)
                        ? match
                        : carreras.FirstOrDefault();
                    actividad = new Actividad(
                        ext.ElementoNombre,
                        ext.ElementoCodigo,
                        periodo.Id,
                        carrera?.Id ?? defaultCarrera.Id,
                        ext.Comision);

                    await _unitOfWork.Actividades.AddAsync(actividad);
                    existing.Add(actividad);
                    existingByCodigo[ext.Comision] = actividad;
                    created++;
                }
            }

            if (ext.DocenteNames.Count > 0)
            {
                await _unitOfWork.Actividades.RemoveAllDocentesAsync(actividad.Id);
                var addedForActividad = new HashSet<string>();
                foreach (var docenteName in ext.DocenteNames)
                {
                    var normalized = docenteName.Trim().ToLowerInvariant();
                    if (nameToUserId.TryGetValue(normalized, out var userId))
                    {
                        var userIdStr = userId.ToString();
                        if (addedForActividad.Add(userIdStr))
                        {
                            await _unitOfWork.ActividadDocentes.AddAsync(new ActividadDocente(actividad.Id, userIdStr));
                            docentesAsignados++;
                        }
                    }
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<SyncActividadesResult>.Success(new SyncActividadesResult(created, existed, noPeriodo, noCarrera, external.Count, docentesAsignados));
    }
}
