using Lienzo.Application.Common.Models;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Commands.CerrarClase;

public record CerrarClaseCommand(Guid ClaseId) : IRequest<Result<bool>>;

public class CerrarClaseCommandHandler : IRequestHandler<CerrarClaseCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CerrarClaseCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(CerrarClaseCommand request, CancellationToken ct)
    {
        var clase = await _unitOfWork.Clases.Query()
            .FirstOrDefaultAsync(c => c.Id == request.ClaseId && !c.IsDeleted, ct);

        if (clase is null)
            return Result<bool>.Failure("Clase no encontrada.");

        if (clase.Estado != Domain.Enums.ClaseEstado.Abierta)
            return Result<bool>.Failure("La clase ya está cerrada.");

        clase.Cerrar();
        _unitOfWork.Clases.Update(clase);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
