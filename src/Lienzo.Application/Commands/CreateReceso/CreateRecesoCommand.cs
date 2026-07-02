using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.CreateReceso;

public record CreateRecesoCommand(CreateRecesoRequest Request) : IRequest<Result<RecesoDto>>;

public class CreateRecesoCommandHandler : IRequestHandler<CreateRecesoCommand, Result<RecesoDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateRecesoCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<RecesoDto>> Handle(CreateRecesoCommand command, CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParse(command.Request.StartDate, out var startDate))
            return Result<RecesoDto>.Failure("Fecha de inicio inválida", "VALIDATION");

        if (!DateOnly.TryParse(command.Request.EndDate, out var endDate))
            return Result<RecesoDto>.Failure("Fecha de fin inválida", "VALIDATION");

        if (endDate < startDate)
            return Result<RecesoDto>.Failure("La fecha de fin debe ser posterior a la fecha de inicio", "VALIDATION");

        if (string.IsNullOrWhiteSpace(command.Request.Description))
            return Result<RecesoDto>.Failure("La descripción es requerida", "VALIDATION");

        var receso = new Receso(startDate, endDate, command.Request.Description);
        await _unitOfWork.Recesos.AddAsync(receso);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RecesoDto>.Success(new RecesoDto(
            receso.Id,
            receso.StartDate.ToString("yyyy-MM-dd"),
            receso.EndDate.ToString("yyyy-MM-dd"),
            receso.Description));
    }
}
