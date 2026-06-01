using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.SyncCarreras;

public record SyncCarrerasCommand : IRequest<Result<SyncCarrerasResult>>;

public record SyncCarrerasResult(int Creados, int Existentes, int TotalExterno);

public class SyncCarrerasCommandHandler : IRequestHandler<SyncCarrerasCommand, Result<SyncCarrerasResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISyncCarreraService _syncService;

    public SyncCarrerasCommandHandler(IUnitOfWork unitOfWork, ISyncCarreraService syncService)
    {
        _unitOfWork = unitOfWork;
        _syncService = syncService;
    }

    public async Task<Result<SyncCarrerasResult>> Handle(SyncCarrerasCommand command, CancellationToken cancellationToken)
    {
        var external = await _syncService.GetCarrerasAsync();
        if (external.Count == 0)
            return Result<SyncCarrerasResult>.Success(new SyncCarrerasResult(0, 0, 0));

        var existing = (await _unitOfWork.Carreras.GetAllAsync()).ToList();
        var existingByCode = existing.Where(c => c.CodigoExterno.HasValue)
            .ToDictionary(c => c.CodigoExterno!.Value);

        var created = 0;
        var existed = 0;

        foreach (var ext in external)
        {
            if (existingByCode.ContainsKey(ext.Propuesta))
            {
                existed++;
                continue;
            }

            var carrera = new Carrera(ext.Nombre, ext.Codigo, ext.Propuesta);
            await _unitOfWork.Carreras.AddAsync(carrera);
            created++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<SyncCarrerasResult>.Success(new SyncCarrerasResult(created, existed, external.Count));
    }
}
