using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.SyncPeriodos;

public record SyncPeriodosCommand(short AnioAcademico) : IRequest<Result<SyncPeriodosResult>>;

public record SyncPeriodosResult(int Creados, int Existentes, int Actualizados, int TotalExterno);

public class SyncPeriodosCommandHandler : IRequestHandler<SyncPeriodosCommand, Result<SyncPeriodosResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISyncPeriodoService _syncService;

    public SyncPeriodosCommandHandler(IUnitOfWork unitOfWork, ISyncPeriodoService syncService)
    {
        _unitOfWork = unitOfWork;
        _syncService = syncService;
    }

    public async Task<Result<SyncPeriodosResult>> Handle(SyncPeriodosCommand command, CancellationToken cancellationToken)
    {
        var external = await _syncService.GetPeriodosAsync(command.AnioAcademico);
        if (external.Count == 0)
            return Result<SyncPeriodosResult>.Success(new SyncPeriodosResult(0, 0, 0, 0));

        var existing = (await _unitOfWork.Periodos.GetAllAsync()).ToList();
        var existingByCode = existing.Where(p => p.CodigoExterno.HasValue)
            .ToDictionary(p => p.CodigoExterno!.Value);

        var tipos = (await _unitOfWork.TiposPeriodo.GetAllAsync()).ToList();

        var created = 0;
        var existed = 0;
        var updated = 0;

        foreach (var ext in external)
        {
            if (existingByCode.TryGetValue(ext.Periodo, out var match))
            {
                existed++;
                continue;
            }

            var normalizedName = ext.Nombre.ToLowerInvariant().Trim();
            var matchByName = existing.FirstOrDefault(p =>
                p.Nombre.ToLowerInvariant().Trim() == normalizedName && p.Anio == ext.AnioAcademico);

            if (matchByName is not null)
            {
                matchByName.SetCodigoExterno(ext.Periodo);
                var tipoId = tipos.FirstOrDefault(t => t.CodigoExterno == ext.PeriodoGenerico)?.Id;
                if (tipoId.HasValue) matchByName.SetTipoPeriodo(tipoId.Value);
                existingByCode[ext.Periodo] = matchByName;
                updated++;
                continue;
            }

            var periodo = new Periodo(ext.Nombre, ext.FechaInicio, ext.FechaFin, ext.AnioAcademico, ext.Periodo);
            var tipoPeriodo = tipos.FirstOrDefault(t => t.CodigoExterno == ext.PeriodoGenerico);
            if (tipoPeriodo is not null)
                periodo.SetTipoPeriodo(tipoPeriodo.Id);

            await _unitOfWork.Periodos.AddAsync(periodo);
            existing.Add(periodo);
            existingByCode[ext.Periodo] = periodo;
            created++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<SyncPeriodosResult>.Success(new SyncPeriodosResult(created, existed, updated, external.Count));
    }
}
