using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.SyncTiposPeriodo;

public record SyncTiposPeriodoCommand : IRequest<Result<SyncTiposPeriodoResult>>;

public record SyncTiposPeriodoResult(int Creados, int Existentes, int TotalExterno);

public class SyncTiposPeriodoCommandHandler : IRequestHandler<SyncTiposPeriodoCommand, Result<SyncTiposPeriodoResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISyncTipoPeriodoService _syncService;

    public SyncTiposPeriodoCommandHandler(IUnitOfWork unitOfWork, ISyncTipoPeriodoService syncService)
    {
        _unitOfWork = unitOfWork;
        _syncService = syncService;
    }

    public async Task<Result<SyncTiposPeriodoResult>> Handle(SyncTiposPeriodoCommand command, CancellationToken cancellationToken)
    {
        var external = await _syncService.GetTiposPeriodoAsync();
        if (external.Count == 0)
            return Result<SyncTiposPeriodoResult>.Success(new SyncTiposPeriodoResult(0, 0, 0));

        var existing = (await _unitOfWork.TiposPeriodo.GetAllAsync()).ToList();
        var existingByCode = existing.Where(t => t.CodigoExterno.HasValue)
            .ToDictionary(t => t.CodigoExterno!.Value);

        var created = 0;
        var existed = 0;

        foreach (var ext in external)
        {
            if (existingByCode.ContainsKey(ext.PeriodoGenerico))
            {
                existed++;
                continue;
            }

            var matchByName = existing.FirstOrDefault(t =>
                t.Nombre.Equals(ext.Nombre, StringComparison.OrdinalIgnoreCase));

            if (matchByName is not null)
            {
                matchByName.SetCodigoExterno(ext.PeriodoGenerico);
                existingByCode[ext.PeriodoGenerico] = matchByName;
                existed++;
                continue;
            }

            var tipo = new TipoPeriodo(ext.Nombre, ext.PeriodoGenerico);
            await _unitOfWork.TiposPeriodo.AddAsync(tipo);
            existing.Add(tipo);
            existingByCode[ext.PeriodoGenerico] = tipo;
            created++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<SyncTiposPeriodoResult>.Success(new SyncTiposPeriodoResult(created, existed, external.Count));
    }
}
