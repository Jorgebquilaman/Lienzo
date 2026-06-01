using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetAllHolidays;

public record GetAllHolidaysQuery : IRequest<Result<List<HolidayDto>>>;

public class GetAllHolidaysQueryHandler : IRequestHandler<GetAllHolidaysQuery, Result<List<HolidayDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllHolidaysQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<HolidayDto>>> Handle(GetAllHolidaysQuery query, CancellationToken cancellationToken)
    {
        var holidays = await _unitOfWork.Holidays.GetAllAsync();
        var dtos = holidays.Select(h => new HolidayDto(h.Id, h.Date.ToString("yyyy-MM-dd"), h.Description)).ToList();
        return Result<List<HolidayDto>>.Success(dtos);
    }
}
