using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetMaintenanceBlocks;

public record GetMaintenanceBlocksQuery(bool? ActiveOnly = null, Guid? ClassroomId = null) : IRequest<Result<MaintenanceBlockListResponse>>;

public class GetMaintenanceBlocksQueryHandler : IRequestHandler<GetMaintenanceBlocksQuery, Result<MaintenanceBlockListResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMaintenanceBlocksQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<MaintenanceBlockListResponse>> Handle(GetMaintenanceBlocksQuery query, CancellationToken ct)
    {
        var blocks = await _unitOfWork.MaintenanceBlocks.GetAllAsync();

        var filtered = blocks.AsEnumerable();

        if (query.ActiveOnly.HasValue)
            filtered = filtered.Where(b => b.IsActive == query.ActiveOnly.Value);
        if (query.ClassroomId.HasValue)
            filtered = filtered.Where(b => b.ClassroomId == query.ClassroomId.Value);

        var ordered = filtered.OrderByDescending(b => b.CreatedAt).ToList();

        var classroomIds = ordered.Select(b => b.ClassroomId).Distinct().ToList();
        var classrooms = (await _unitOfWork.Classrooms.GetAllAsync())
            .Where(c => classroomIds.Contains(c.Id))
            .ToDictionary(c => c.Id, c => c);
        var buildingIds = classrooms.Values.Select(c => c.BuildingId).Distinct().ToList();
        var buildings = (await _unitOfWork.Buildings.GetAllAsync())
            .Where(b => buildingIds.Contains(b.Id))
            .ToDictionary(b => b.Id, b => b.Name);

        var items = ordered.Select(b =>
        {
            var cls = classrooms.GetValueOrDefault(b.ClassroomId);
            return new MaintenanceBlockDto(
                b.Id, b.ClassroomId,
                cls?.Name ?? "",
                cls is not null && buildings.TryGetValue(cls.BuildingId, out var bn) ? bn : null,
                b.StartTime, b.EndTime, b.Reason,
                b.CreatedBy.ToString(), b.CreatedAt);
        }).ToList();

        return Result<MaintenanceBlockListResponse>.Success(new(items, items.Count));
    }
}
