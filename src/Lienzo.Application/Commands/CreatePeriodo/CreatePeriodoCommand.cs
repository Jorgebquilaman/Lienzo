using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.CreatePeriodo;

public record CreatePeriodoCommand(CreatePeriodoRequest Request) : IRequest<Result<PeriodoDto>>;

public class CreatePeriodoCommandHandler : IRequestHandler<CreatePeriodoCommand, Result<PeriodoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreatePeriodoCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PeriodoDto>> Handle(CreatePeriodoCommand command, CancellationToken ct)
    {
        if (!DateOnly.TryParse(command.Request.FechaInicio, out var fechaInicio))
            return Result<PeriodoDto>.Failure("Fecha de inicio inválida", "VALIDATION");
        if (!DateOnly.TryParse(command.Request.FechaFin, out var fechaFin))
            return Result<PeriodoDto>.Failure("Fecha de fin inválida", "VALIDATION");

        var periodo = new Periodo(command.Request.Nombre, fechaInicio, fechaFin, command.Request.Anio);
        await _unitOfWork.Periodos.AddAsync(periodo);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<PeriodoDto>.Success(new PeriodoDto(periodo.Id, periodo.Nombre, periodo.FechaInicio.ToString("yyyy-MM-dd"), periodo.FechaFin.ToString("yyyy-MM-dd"), periodo.Anio, periodo.CodigoExterno, periodo.TipoPeriodoId, null));
    }
}
