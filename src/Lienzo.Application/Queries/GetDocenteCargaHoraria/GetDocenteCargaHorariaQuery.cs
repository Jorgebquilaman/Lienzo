using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetDocenteCargaHoraria;

public record GetDocenteCargaHorariaQuery(DocenteCargaHorariaFilter Filter) : IRequest<Result<DocenteCargaHorariaResponse>>;

public class GetDocenteCargaHorariaQueryHandler : IRequestHandler<GetDocenteCargaHorariaQuery, Result<DocenteCargaHorariaResponse>>
{
    private readonly IReportService _reportService;

    public GetDocenteCargaHorariaQueryHandler(IReportService reportService) => _reportService = reportService;

    public async Task<Result<DocenteCargaHorariaResponse>> Handle(GetDocenteCargaHorariaQuery query, CancellationToken ct)
        => await _reportService.GetDocenteCargaHorariaAsync(query.Filter);
}
