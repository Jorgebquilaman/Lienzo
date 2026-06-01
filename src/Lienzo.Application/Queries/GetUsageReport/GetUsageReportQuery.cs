using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetUsageReport;

public record GetUsageReportQuery(UsageReportFilter Filter) : IRequest<Result<UsageReportResponse>>;

public class GetUsageReportQueryHandler : IRequestHandler<GetUsageReportQuery, Result<UsageReportResponse>>
{
    private readonly IReportService _reportService;

    public GetUsageReportQueryHandler(IReportService reportService) => _reportService = reportService;

    public async Task<Result<UsageReportResponse>> Handle(GetUsageReportQuery query, CancellationToken ct)
        => await _reportService.GetUsageReportAsync(query.Filter);
}
