namespace Lienzo.Application.DTOs;

public record HolidayDto(Guid Id, string Date, string Description);

public record CreateHolidayRequest(string Date, string Description);

public record UpdateHolidayRequest(string Date, string Description);
