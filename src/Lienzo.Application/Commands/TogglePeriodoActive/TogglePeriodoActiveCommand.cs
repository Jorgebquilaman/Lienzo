using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.TogglePeriodoActive;

public record TogglePeriodoActiveCommand(Guid Id) : IRequest<Result<PeriodoDto>>;

public class TogglePeriodoActiveCommandHandler : IRequestHandler<TogglePeriodoActiveCommand, Result<PeriodoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TogglePeriodoActiveCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PeriodoDto>> Handle(TogglePeriodoActiveCommand command, CancellationToken ct)
    {
        var periodo = await _unitOfWork.Periodos.GetByIdAsync(command.Id);
        if (periodo is null)
            return Result<PeriodoDto>.Failure("Periodo no encontrado", "NOT_FOUND");

        periodo.ToggleActive();
        _unitOfWork.Periodos.Update(periodo);
        await _unitOfWork.SaveChangesAsync(ct);

        var dto = _mapper.Map<PeriodoDto>(periodo);
        return Result<PeriodoDto>.Success(dto);
    }
}
