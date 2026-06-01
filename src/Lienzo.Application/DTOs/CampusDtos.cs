namespace Lienzo.Application.DTOs;

public record CampusStatusResponse(
    List<CampusBuildingDto> Buildings,
    DateTime Timestamp);

public record CampusBuildingDto(
    Guid Id,
    string Name,
    List<CampusFloorDto> Floors);

public record CampusFloorDto(
    int Floor,
    List<CampusClassroomStatusDto> Classrooms);

public record CampusClassroomStatusDto(
    Guid Id,
    string Name,
    int Capacity,
    string Type,
    string Status,
    CampusReservationInfo? CurrentReservation);

public record CampusReservationInfo(
    Guid ReservationId,
    string Title,
    string UserName,
    TimeOnly StartTime,
    TimeOnly EndTime);
