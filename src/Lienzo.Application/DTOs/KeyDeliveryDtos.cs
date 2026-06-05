namespace Lienzo.Application.DTOs;

public record KeyDeliveryDto(
    Guid Id,
    Guid ClassroomId,
    string ClassroomName,
    string? BuildingName,
    Guid? DeliveredToUserId,
    string DeliveredToName,
    Guid DeliveredById,
    string DeliveredByName,
    DateTime DeliveredAt,
    DateTime? ReturnedAt,
    string? Notes,
    List<AccessoryDto>? Accessories = null);

public record KeyDeliveryActiveDto(
    Guid Id,
    Guid ClassroomId,
    string ClassroomName,
    string? BuildingName,
    Guid? DeliveredToUserId,
    string DeliveredToName,
    DateTime DeliveredAt,
    string? Notes,
    NextReservationInfo? NextReservation,
    List<AccessoryDto>? Accessories = null) : KeyDeliveryDto(Id, ClassroomId, ClassroomName, BuildingName, DeliveredToUserId, DeliveredToName, Guid.Empty, "", DeliveredAt, null, Notes, Accessories);

public record NextReservationInfo(
    Guid ReservationId,
    string ProfessorName,
    Guid ProfessorUserId,
    string StartTime,
    string EndTime);

public record DeliverKeyRequest(
    Guid ClassroomId,
    Guid? DeliveredToUserId,
    string DeliveredToName,
    string? Notes,
    List<Guid>? AccessoryIds = null);

public record KeyDeliveryListResponse(
    List<KeyDeliveryDto> Items,
    int TotalCount);
