using Lienzo.Application.Common.Models;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.DeleteHoliday;

public record DeleteHolidayCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteHolidayCommandHandler : IRequestHandler<DeleteHolidayCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteHolidayCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(DeleteHolidayCommand command, CancellationToken cancellationToken)
    {
        var holiday = await _unitOfWork.Holidays.GetByIdAsync(command.Id);
        if (holiday is null)
            return Result<bool>.Failure("Feriado no encontrado", "NOT_FOUND");

        _unitOfWork.Holidays.Delete(holiday);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
