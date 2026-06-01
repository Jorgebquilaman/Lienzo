using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetDemandMetrics;

public record GetDemandMetricsQuery(DateOnly? FromDate, DateOnly? ToDate) : IRequest<Result<DemandMetricsResponse>>;

public class GetDemandMetricsQueryHandler : IRequestHandler<GetDemandMetricsQuery, Result<DemandMetricsResponse>>
{
    private readonly IReportService _reportService;

    public GetDemandMetricsQueryHandler(IReportService reportService) => _reportService = reportService;

    public async Task<Result<DemandMetricsResponse>> Handle(GetDemandMetricsQuery query, CancellationToken ct)
        => await _reportService.GetDemandMetricsAsync(query.FromDate, query.ToDate);
}
