using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetUsageByProposal;

public record GetUsageByProposalQuery(UsageByProposalFilter Filter) : IRequest<Result<UsageByProposalResponse>>;

public class GetUsageByProposalQueryHandler : IRequestHandler<GetUsageByProposalQuery, Result<UsageByProposalResponse>>
{
    private readonly IReportService _reportService;

    public GetUsageByProposalQueryHandler(IReportService reportService) => _reportService = reportService;

    public async Task<Result<UsageByProposalResponse>> Handle(GetUsageByProposalQuery query, CancellationToken ct)
        => await _reportService.GetUsageByProposalAsync(query.Filter);
}
