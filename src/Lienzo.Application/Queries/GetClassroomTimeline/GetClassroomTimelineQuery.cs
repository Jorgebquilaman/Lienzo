using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetClassroomTimeline;

public record GetClassroomTimelineQuery(ClassroomTimelineFilter Filter) : IRequest<Result<ClassroomTimelineResponse>>;

public class GetClassroomTimelineQueryHandler : IRequestHandler<GetClassroomTimelineQuery, Result<ClassroomTimelineResponse>>
{
    private readonly IReportService _reportService;

    public GetClassroomTimelineQueryHandler(IReportService reportService) => _reportService = reportService;

    public async Task<Result<ClassroomTimelineResponse>> Handle(GetClassroomTimelineQuery query, CancellationToken ct)
        => await _reportService.GetClassroomTimelineAsync(query.Filter);
}
