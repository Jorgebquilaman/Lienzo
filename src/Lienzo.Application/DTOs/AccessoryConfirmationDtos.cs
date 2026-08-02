namespace Lienzo.Application.DTOs;

public record AccessoryConfirmationOptionDto(string Name, string Origin);

public record AccessoryConfirmationDto(
    string Token,
    string ClassroomName,
    DateTime Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Title,
    bool AlreadyConfirmed,
    List<AccessoryConfirmationOptionDto> Accessories);

public record ConfirmAccessoriesRequest(string Token, List<string> RequestedAccessories);

public record ReservationAccessoryDto(
    string Name,
    string Origin,
    bool IsRequested,
    bool? IsGranted);

public record ReservationAccessoriesDto(
    Guid ReservationId,
    bool RequiresConfirmation,
    bool Confirmed,
    List<ReservationAccessoryDto> Accessories);

public record DecideAccessoryRequest(string Name, bool Granted);
