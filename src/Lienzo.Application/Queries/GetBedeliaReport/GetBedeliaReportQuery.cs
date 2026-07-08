using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetBedeliaReport;

public record GetBedeliaReportQuery(DateOnly? FromDate, DateOnly? ToDate) : IRequest<Result<BedeliaReportResponse>>;

public class GetBedeliaReportQueryHandler : IRequestHandler<GetBedeliaReportQuery, Result<BedeliaReportResponse>>
{
    private readonly IReportService _reportService;

    public GetBedeliaReportQueryHandler(IReportService reportService) => _reportService = reportService;

    public async Task<Result<BedeliaReportResponse>> Handle(GetBedeliaReportQuery query, CancellationToken ct)
        => await _reportService.GetBedeliaReportAsync(query.FromDate, query.ToDate);
}
