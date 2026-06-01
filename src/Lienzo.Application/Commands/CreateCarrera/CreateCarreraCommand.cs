using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.CreateCarrera;

public record CreateCarreraCommand(CreateCarreraRequest Request) : IRequest<Result<CarreraDto>>;

public class CreateCarreraCommandHandler : IRequestHandler<CreateCarreraCommand, Result<CarreraDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateCarreraCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<CarreraDto>> Handle(CreateCarreraCommand command, CancellationToken ct)
    {
        var carrera = new Carrera(command.Request.Nombre, command.Request.Codigo);
        await _unitOfWork.Carreras.AddAsync(carrera);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<CarreraDto>.Success(new CarreraDto(carrera.Id, carrera.Nombre, carrera.Codigo));
    }
}
