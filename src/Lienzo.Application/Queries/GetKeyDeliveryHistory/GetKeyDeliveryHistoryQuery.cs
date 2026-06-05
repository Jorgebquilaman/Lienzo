using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Queries.GetKeyDeliveryHistory;

public record GetKeyDeliveryHistoryQuery(Guid? ClassroomId = null) : IRequest<Result<KeyDeliveryListResponse>>;

public class GetKeyDeliveryHistoryQueryHandler : IRequestHandler<GetKeyDeliveryHistoryQuery, Result<KeyDeliveryListResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public GetKeyDeliveryHistoryQueryHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<Result<KeyDeliveryListResponse>> Handle(GetKeyDeliveryHistoryQuery query, CancellationToken ct)
    {
        IQueryable<KeyDelivery> queryable = _unitOfWork.KeyDeliveries.Query()
            .Where(d => d.ReturnedAt != null)
            .Include(d => d.Accessories)
            .ThenInclude(a => a.Accessory);

        if (query.ClassroomId.HasValue)
            queryable = queryable.Where(d => d.ClassroomId == query.ClassroomId.Value);

        var ordered = await queryable
            .OrderByDescending(d => d.DeliveredAt)
            .ToListAsync(ct);

        if (ordered.Count == 0)
            return Result<KeyDeliveryListResponse>.Success(new(new(), 0));

        var classroomIds = ordered.Select(d => d.ClassroomId).Distinct().ToList();
        var classrooms = (await _unitOfWork.Classrooms.GetAllAsync())
            .Where(c => classroomIds.Contains(c.Id))
            .ToDictionary(c => c.Id, c => c);
        var buildingIds = classrooms.Values.Select(c => c.BuildingId).Distinct().ToList();
        var buildings = (await _unitOfWork.Buildings.GetAllAsync())
            .Where(b => buildingIds.Contains(b.Id))
            .ToDictionary(b => b.Id, b => b.Name);

        var userIds = ordered.Select(d => d.DeliveredById).Distinct().ToList();
        var usersResult = await _authService.GetAllUsersAsync();
        var userNames = usersResult.IsSuccess
            ? usersResult.Value.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}")
            : new Dictionary<Guid, string>();

        var items = ordered.Select(d =>
        {
            var cls = classrooms.GetValueOrDefault(d.ClassroomId);
            var accessories = d.Accessories?
                .Where(a => a.Accessory != null)
                .Select(a => new AccessoryDto(a.Accessory.Id, a.Accessory.Name, a.Accessory.Description, a.Accessory.IsActive))
                .ToList();
            return new KeyDeliveryDto(
                d.Id, d.ClassroomId,
                cls?.Name ?? "",
                cls is not null && buildings.TryGetValue(cls.BuildingId, out var bn) ? bn : null,
                d.DeliveredToUserId, d.DeliveredToName,
                d.DeliveredById,
                userNames.GetValueOrDefault(d.DeliveredById, "Desconocido"),
                d.DeliveredAt, d.ReturnedAt, d.Notes,
                accessories?.Count > 0 ? accessories : null);
        }).ToList();

        return Result<KeyDeliveryListResponse>.Success(new(items, items.Count));
    }
}
