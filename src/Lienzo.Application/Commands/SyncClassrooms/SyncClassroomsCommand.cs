using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.SyncClassrooms;

public record SyncClassroomsCommand : IRequest<Result<SyncClassroomsResult>>;

public record SyncClassroomsResult(int Creados, int Existentes, int SinEdificio, int TotalExterno);

public class SyncClassroomsCommandHandler : IRequestHandler<SyncClassroomsCommand, Result<SyncClassroomsResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISyncClassroomService _syncService;

    public SyncClassroomsCommandHandler(IUnitOfWork unitOfWork, ISyncClassroomService syncService)
    {
        _unitOfWork = unitOfWork;
        _syncService = syncService;
    }

    public async Task<Result<SyncClassroomsResult>> Handle(SyncClassroomsCommand command, CancellationToken cancellationToken)
    {
        var externalClassrooms = await _syncService.GetExternalClassroomsAsync();
        if (externalClassrooms.Count == 0)
            return Result<SyncClassroomsResult>.Success(new SyncClassroomsResult(0, 0, 0, 0));

        var buildings = (await _unitOfWork.Buildings.GetAllAsync()).ToList();
        var existingClassrooms = (await _unitOfWork.Classrooms.GetAllAsync())
            .Select(c => (c.Name.ToLowerInvariant().Trim(), c.BuildingId))
            .ToHashSet();

        var created = 0;
        var existed = 0;
        var noBuilding = 0;

        foreach (var ext in externalClassrooms)
        {
            var building = buildings.FirstOrDefault(b => b.CodigoExterno == ext.Edificacion);
            if (building is null)
            {
                noBuilding++;
                continue;
            }

            var normalizedName = ext.Nombre.ToLowerInvariant().Trim();
            if (existingClassrooms.Contains((normalizedName, building.Id)))
            {
                existed++;
                continue;
            }

            var floor = TryParseFloor(ext.Piso);
            var capacity = ext.Capacidad.HasValue ? ext.Capacidad.Value : 30;

            var classroom = new Classroom(
                ext.Nombre,
                building.Id,
                floor,
                capacity,
                ClassroomType.General);

            await _unitOfWork.Classrooms.AddAsync(classroom);
            existingClassrooms.Add((normalizedName, building.Id));
            created++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SyncClassroomsResult>.Success(new SyncClassroomsResult(created, existed, noBuilding, externalClassrooms.Count));
    }

    private static int TryParseFloor(string? piso)
    {
        if (string.IsNullOrWhiteSpace(piso)) return 1;

        piso = piso.ToLowerInvariant().Trim()
            .Replace("planta baja", "0")
            .Replace("piso ", "")
            .Replace("°", "")
            .Trim();

        if (int.TryParse(piso, out var floor))
            return floor;

        return 1;
    }
}
