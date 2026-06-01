using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.CreateHoliday;

public record CreateHolidayCommand(CreateHolidayRequest Request) : IRequest<Result<HolidayDto>>;

public class CreateHolidayCommandHandler : IRequestHandler<CreateHolidayCommand, Result<HolidayDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateHolidayCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<HolidayDto>> Handle(CreateHolidayCommand command, CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParse(command.Request.Date, out var date))
            return Result<HolidayDto>.Failure("Fecha inválida", "VALIDATION");

        var holiday = new Holiday(date, command.Request.Description);
        await _unitOfWork.Holidays.AddAsync(holiday);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<HolidayDto>.Success(new HolidayDto(holiday.Id, holiday.Date.ToString("yyyy-MM-dd"), holiday.Description));
    }
}
