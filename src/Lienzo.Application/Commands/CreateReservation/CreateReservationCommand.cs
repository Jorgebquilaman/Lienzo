using System.Text.Json;
using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.CreateReservation;

public record CreateReservationCommand(CreateReservationRequest Request) : IRequest<Result<ReservationDto>>;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Result<ReservationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;

    public CreateReservationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _authService = authService;
    }

    public async Task<Result<ReservationDto>> Handle(CreateReservationCommand command, CancellationToken cancellationToken)
    {
        var classroom = await _unitOfWork.Classrooms.GetWithReservationsAsync(command.Request.ClassroomId);
        if (classroom is null || classroom.IsDeleted)
            return Result<ReservationDto>.Failure("Classroom not found", "NOT_FOUND");

        if (!classroom.IsActive)
            return Result<ReservationDto>.Failure("Classroom is not active", "INACTIVE");

        if (command.Request.EndDate.HasValue && command.Request.EndDate.Value < command.Request.Date)
            return Result<ReservationDto>.Failure("End date must be after start date", "INVALID_DATES");

        var startDate = command.Request.Date;
        var dates = GetDates(startDate, command.Request.DaysOfWeek, command.Request.EndDate);

        foreach (var date in dates)
        {
            if (date.DayOfWeek == DayOfWeek.Sunday)
                return Result<ReservationDto>.Failure("No se permiten reservas los domingos", "HOLIDAY");

            if (date.DayOfWeek == DayOfWeek.Saturday)
            {
                if (command.Request.StartTime >= new TimeOnly(16, 0))
                    return Result<ReservationDto>.Failure("Los sábados solo se permite reservar hasta las 16:00", "HOLIDAY");

                if (command.Request.EndTime > new TimeOnly(16, 0))
                    return Result<ReservationDto>.Failure("Los sábados la reserva debe terminar antes de las 16:00", "HOLIDAY");
            }

            if (await _unitOfWork.Holidays.IsHolidayAsync(date))
                return Result<ReservationDto>.Failure("No se permiten reservas en días feriados", "HOLIDAY");
        }

        var maintenanceBlocks = await _unitOfWork.MaintenanceBlocks.GetAllAsync();
        var activeBlocks = maintenanceBlocks
            .Where(m => m.ClassroomId == command.Request.ClassroomId && m.IsActive)
            .ToList();

        foreach (var date in dates)
        {
            var resStart = date.ToDateTime(command.Request.StartTime);
            var resEnd = date.ToDateTime(command.Request.EndTime);
            if (activeBlocks.Any(m =>
                m.StartTime.ToLocalTime() < resEnd &&
                m.EndTime.ToLocalTime() > resStart))
                return Result<ReservationDto>.Failure(
                    "El aula está en mantenimiento en el horario solicitado", "MAINTENANCE");
        }

        var hasConflict = await _unitOfWork.Reservations.HasConflictForDatesAsync(
            command.Request.ClassroomId,
            dates,
            command.Request.StartTime,
            command.Request.EndTime);

        if (hasConflict)
            return Result<ReservationDto>.Failure("El aula tiene un conflicto en uno o más de los horarios solicitados", "CONFLICT");

        Guid? recurringGroupId = dates.Count > 1 ? Guid.NewGuid() : null;
        string? recurrenceRule = null;

        if (recurringGroupId.HasValue)
        {
            var rule = new
            {
                daysOfWeek = command.Request.DaysOfWeek?.Split(',').Select(d => d.Trim()).ToList(),
                endDate = command.Request.EndDate?.ToString("yyyy-MM-dd")
            };
            recurrenceRule = JsonSerializer.Serialize(rule);
        }

        var reservations = new List<Reservation>();
        foreach (var date in dates)
        {
            var reservation = Reservation.Create(
                command.Request.ClassroomId,
                _currentUser.UserId,
                command.Request.Title,
                command.Request.Description,
                date,
                command.Request.StartTime,
                command.Request.EndTime,
                recurringGroupId,
                recurrenceRule,
                command.Request.ActividadId);

            await _unitOfWork.Reservations.AddAsync(reservation);
            reservations.Add(reservation);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<ReservationDto>(reservations.First());
        dto.ClassroomName = classroom.Name;
        dto.Date = command.Request.Date.ToDateTime(TimeOnly.MinValue);

        if (command.Request.ActividadId.HasValue)
        {
            var actividad = await _unitOfWork.Actividades.GetWithDetailsAsync(command.Request.ActividadId.Value);
            if (actividad is not null)
            {
                dto.ActividadNombre = actividad.Nombre;
                dto.ActividadPeriodo = actividad.Periodo?.Nombre;
                dto.ActividadCarrera = actividad.Carrera?.Nombre;

                var usersResult = await _authService.GetAllUsersAsync();
                var userMap = usersResult.IsSuccess
                    ? usersResult.Value.ToDictionary(u => u.Id.ToString(), u => $"{u.FirstName} {u.LastName}")
                    : new Dictionary<string, string>();
                dto.ActividadDocentes = string.Join(", ", actividad.Docentes.Select(d => userMap.GetValueOrDefault(d.DocenteId, d.DocenteId)).Distinct());
            }
        }

        return Result<ReservationDto>.Success(dto);
    }

    private static List<DateOnly> GetDates(DateOnly startDate, string? daysOfWeek, DateOnly? endDate)
    {
        if (string.IsNullOrWhiteSpace(daysOfWeek) || !endDate.HasValue)
            return [startDate];

        var end = endDate.Value;
        if (end <= startDate)
            return [startDate];

        var days = daysOfWeek
            .Split(',')
            .Select(d => Enum.Parse<DayOfWeek>(d.Trim(), ignoreCase: true))
            .ToHashSet();

        var dates = new List<DateOnly>();
        for (var date = startDate; date <= end; date = date.AddDays(1))
        {
            if (days.Contains(date.DayOfWeek))
                dates.Add(date);
        }

        return dates.Count > 0 ? dates : [startDate];
    }
}
