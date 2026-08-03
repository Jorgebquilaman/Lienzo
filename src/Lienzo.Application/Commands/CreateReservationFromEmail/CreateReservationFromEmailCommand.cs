using System.Text.Json;
using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Commands.CreateReservationFromEmail;

public record CreateReservationFromEmailCommand(CreateReservationFromEmailRequest Request) : IRequest<Result<ReservationDto>>;

public class CreateReservationFromEmailCommandHandler : IRequestHandler<CreateReservationFromEmailCommand, Result<ReservationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;
    private readonly IEmailReaderService _emailReader;
    private readonly IEmailEvidenceStorage _evidenceStorage;

    public CreateReservationFromEmailCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUser,
        IAuthService authService,
        IEmailReaderService emailReader,
        IEmailEvidenceStorage evidenceStorage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _authService = authService;
        _emailReader = emailReader;
        _evidenceStorage = evidenceStorage;
    }

    public async Task<Result<ReservationDto>> Handle(CreateReservationFromEmailCommand command, CancellationToken ct)
    {
        var alreadyProcessed = await _unitOfWork.ProcessedEmails.Query()
            .AnyAsync(p => p.EmailUid == command.Request.EmailUid && !p.IsDeleted, ct);
        if (alreadyProcessed)
            return Result<ReservationDto>.Failure("Este correo ya fue procesado para una reserva.", "EMAIL_ALREADY_PROCESSED");

        var usersResult = await _authService.GetAllUsersAsync();
        if (!usersResult.IsSuccess)
            return Result<ReservationDto>.Failure("No se pudo validar el usuario asignado.");

        var assignedUser = usersResult.Value.FirstOrDefault(u => u.Id == command.Request.AssignedUserId);
        if (assignedUser is null)
            return Result<ReservationDto>.Failure("El usuario asignado no existe.", "USER_NOT_FOUND");

        var userMap = usersResult.Value.ToDictionary(u => u.Id.ToString(), u => $"{u.FirstName} {u.LastName}");

        var classroom = await _unitOfWork.Classrooms.GetWithReservationsAsync(command.Request.ClassroomId);
        if (classroom is null || classroom.IsDeleted)
            return Result<ReservationDto>.Failure("Classroom not found", "NOT_FOUND");

        if (!classroom.IsActive)
            return Result<ReservationDto>.Failure("Classroom is not active", "INACTIVE");

        if (command.Request.EndDate.HasValue && command.Request.EndDate.Value < command.Request.Date)
            return Result<ReservationDto>.Failure("End date must be after start date", "INVALID_DATES");

        var startDate = command.Request.Date;
        if (startDate < DateOnly.FromDateTime(DateTime.Now))
            return Result<ReservationDto>.Failure("La fecha de la reserva no puede ser en el pasado", "INVALID_DATE");

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

            if (await _unitOfWork.Recesos.IsRecesoAsync(date))
                return Result<ReservationDto>.Failure("No se permiten reservas en días de receso académico", "RECESS");
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

        var emailDetail = await _emailReader.GetMessageAsync(command.Request.EmailUid, ct);
        if (emailDetail.IsFailure)
            return Result<ReservationDto>.Failure($"No se pudo obtener el correo de origen: {emailDetail.Error}");

        var email = emailDetail.Value;

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

        var evidencePath = await SaveEvidenceAsync(command.Request.EmailUid, emailDetail.Value.Uid, ct);

        var reservations = new List<Reservation>();
        foreach (var date in dates)
        {
            try
            {
                var reservation = Reservation.Create(
                    command.Request.ClassroomId,
                    command.Request.AssignedUserId,
                    command.Request.Title,
                    command.Request.Description,
                    date,
                    command.Request.StartTime,
                    command.Request.EndTime,
                    recurringGroupId,
                    recurrenceRule,
                    command.Request.ActividadId,
                    command.Request.EmailUid,
                    email.From,
                    email.Subject,
                    email.Date.ToUniversalTime().DateTime,
                    evidencePath);

                await _unitOfWork.Reservations.AddAsync(reservation);

                if (!command.Request.RequestAccessoryConfirmation)
                {
                    try
                    {
                        reservation.Approve(_currentUser.UserId);
                    }
                    catch (InvalidOperationException)
                    {
                        return Result<ReservationDto>.Failure("No se pudo autorizar la reserva automáticamente.", "VALIDATION");
                    }
                }

                reservations.Add(reservation);
            }
            catch (ArgumentException ex)
            {
                return Result<ReservationDto>.Failure(ex.Message, "VALIDATION");
            }
        }

        var firstReservation = reservations.First();
        await _unitOfWork.ProcessedEmails.AddAsync(new ProcessedEmail(
            command.Request.EmailUid,
            firstReservation.Id,
            _currentUser.UserId));

        await _unitOfWork.SaveChangesAsync(ct);

        var dto = _mapper.Map<ReservationDto>(firstReservation);
        dto.ClassroomName = classroom.Name;
        dto.Date = command.Request.Date.ToDateTime(TimeOnly.MinValue);
        dto.UserName = userMap.GetValueOrDefault(dto.UserId.ToString(), "");

        if (command.Request.ActividadId.HasValue)
        {
            var actividad = await _unitOfWork.Actividades.GetWithDetailsAsync(command.Request.ActividadId.Value);
            if (actividad is not null)
            {
                dto.ActividadNombre = actividad.Nombre;
                dto.ActividadPeriodo = actividad.Periodo?.Nombre;
                dto.ActividadCarrera = actividad.Carrera?.Nombre;
                dto.ActividadDocentes = string.Join(", ", actividad.Docentes.Select(d => userMap.GetValueOrDefault(d.DocenteId, d.DocenteId)).Distinct());
            }
        }

        return Result<ReservationDto>.Success(dto);
    }

    private async Task<string?> SaveEvidenceAsync(string uid, string fileName, CancellationToken ct)
    {
        try
        {
            var raw = await _emailReader.DownloadRawAsync(uid, ct);
            if (raw.IsFailure || raw.Value is null || raw.Value.Length == 0)
                return null;

            var reservationId = Guid.NewGuid();
            return await _evidenceStorage.SaveAsync(raw.Value, $"{reservationId:N}.eml", ct);
        }
        catch
        {
            return null;
        }
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
