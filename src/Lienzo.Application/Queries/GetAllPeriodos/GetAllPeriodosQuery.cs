using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetAllPeriodos;

public record GetAllPeriodosQuery : IRequest<Result<List<PeriodoDto>>>;

public class GetAllPeriodosQueryHandler : IRequestHandler<GetAllPeriodosQuery, Result<List<PeriodoDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllPeriodosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<PeriodoDto>>> Handle(GetAllPeriodosQuery query, CancellationToken ct)
    {
        var items = await _unitOfWork.Periodos.GetAllAsync();
        var dtos = _mapper.Map<List<PeriodoDto>>(items.Where(p => !p.IsDeleted).ToList());
        return Result<List<PeriodoDto>>.Success(dtos);
    }
}
