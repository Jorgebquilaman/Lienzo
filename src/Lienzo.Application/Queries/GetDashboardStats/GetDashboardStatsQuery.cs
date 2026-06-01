using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetDashboardStats;

public record GetDashboardStatsQuery : IRequest<Result<DashboardStatsDto>>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDashboardService _dashboardService;

    public GetDashboardStatsQueryHandler(IUnitOfWork unitOfWork, IDashboardService dashboardService)
    {
        _unitOfWork = unitOfWork;
        _dashboardService = dashboardService;
    }

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery query, CancellationToken cancellationToken)
    {
        var stats = await _dashboardService.GetStatsAsync();
        return stats;
    }
}
