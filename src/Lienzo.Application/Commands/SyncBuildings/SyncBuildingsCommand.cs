using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.SyncBuildings;

public record SyncBuildingsCommand : IRequest<Result<SyncBuildingsResult>>;

public record SyncBuildingsResult(int Creados, int Existentes, int Actualizados, int TotalExterno);

public class SyncBuildingsCommandHandler : IRequestHandler<SyncBuildingsCommand, Result<SyncBuildingsResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISyncBuildingService _syncService;

    public SyncBuildingsCommandHandler(IUnitOfWork unitOfWork, ISyncBuildingService syncService)
    {
        _unitOfWork = unitOfWork;
        _syncService = syncService;
    }

    public async Task<Result<SyncBuildingsResult>> Handle(SyncBuildingsCommand command, CancellationToken cancellationToken)
    {
        var externalBuildings = await _syncService.GetExternalBuildingsAsync();
        if (externalBuildings.Count == 0)
            return Result<SyncBuildingsResult>.Success(new SyncBuildingsResult(0, 0, 0, 0));

        var existing = (await _unitOfWork.Buildings.GetAllAsync()).ToList();
        var existingByCode = existing.Where(b => b.CodigoExterno.HasValue)
            .ToDictionary(b => b.CodigoExterno!.Value);

        var created = 0;
        var existed = 0;
        var updated = 0;

        foreach (var ext in externalBuildings)
        {
            if (existingByCode.TryGetValue(ext.Edificacion, out var matchByCode))
            {
                existed++;
                continue;
            }

            var normalizedName = ext.Nombre.ToLowerInvariant().Trim();
            var matchByName = existing.FirstOrDefault(b =>
                b.Name.ToLowerInvariant().Trim() == normalizedName);

            if (matchByName is not null)
            {
                matchByName.SetCodigoExterno(ext.Edificacion);
                existingByCode[ext.Edificacion] = matchByName;
                updated++;
                continue;
            }

            var building = new Building(ext.Nombre, ext.Nombre, 1, ext.Edificacion);
            await _unitOfWork.Buildings.AddAsync(building);
            existing.Add(building);
            existingByCode[ext.Edificacion] = building;
            created++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SyncBuildingsResult>.Success(new SyncBuildingsResult(created, existed, updated, externalBuildings.Count));
    }
}
