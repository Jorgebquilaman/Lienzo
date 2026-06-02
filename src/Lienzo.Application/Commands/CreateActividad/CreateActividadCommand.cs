using System.Text.Json;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.CreateActividad;

public record CreateActividadCommand(CreateActividadRequest Request) : IRequest<Result<ActividadDto>>;

public class CreateActividadCommandHandler : IRequestHandler<CreateActividadCommand, Result<ActividadDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public CreateActividadCommandHandler(IUnitOfWork unitOfWork, IAuthService authService, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _currentUser = currentUser;
    }

    public async Task<Result<ActividadDto>> Handle(CreateActividadCommand command, CancellationToken ct)
    {
        var actividad = new Actividad(
            command.Request.Nombre,
            command.Request.CodigoMateria,
            command.Request.PeriodoId,
            command.Request.CarreraId);

        if (command.Request.DocenteIds is not null)
        {
            foreach (var docenteId in command.Request.DocenteIds)
                actividad.AddDocente(docenteId);
        }

        if (command.Request.AulaId.HasValue && command.Request.DiaSemana is not null
            && command.Request.HoraInicio is not null && command.Request.HoraFin is not null)
        {
            if (!TimeOnly.TryParse(command.Request.HoraInicio, out var hi))
                return Result<ActividadDto>.Failure("Hora inicio inválida", "VALIDATION");
            if (!TimeOnly.TryParse(command.Request.HoraFin, out var hf))
                return Result<ActividadDto>.Failure("Hora fin inválida", "VALIDATION");
            actividad.AssignSchedule(command.Request.AulaId.Value, command.Request.DiaSemana, hi, hf);
        }

        await _unitOfWork.Actividades.AddAsync(actividad);
        await _unitOfWork.SaveChangesAsync(ct);

        // Generate reservations from schedule
        var saved = await _unitOfWork.Actividades.GetWithDetailsAsync(actividad.Id);
        if (saved is not null && saved.AulaId.HasValue && saved.DiaSemana is not null
            && saved.HoraInicio.HasValue && saved.HoraFin.HasValue)
        {
            await GenerateReservationsFromScheduleAsync(saved);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        saved = await _unitOfWork.Actividades.GetWithDetailsAsync(actividad.Id);
        var docenteNames = await ResolveDocenteNames(saved!.Docentes.Select(d => d.DocenteId).ToList());
        return Result<ActividadDto>.Success(new ActividadDto(
            saved.Id, saved.Nombre, saved.CodigoMateria,
            saved.PeriodoId, saved.Periodo?.Nombre,
            saved.CarreraId, saved.Carrera?.Nombre,
            saved.AulaId, saved.Aula?.Name,
            saved.DiaSemana, saved.HoraInicio?.ToString("HH:mm"), saved.HoraFin?.ToString("HH:mm"),
            saved.Docentes.Select(d => d.DocenteId).ToList(),
            docenteNames,
            saved.ComisionNombre
        ));
    }

    private async Task<string> ResolveDocenteNames(List<string> userIds)
    {
        var usersResult = await _authService.GetAllUsersAsync();
        if (!usersResult.IsSuccess) return string.Join(", ", userIds);
        var userMap = usersResult.Value.ToDictionary(u => u.Id.ToString(), u => $"{u.FirstName} {u.LastName}");
        return string.Join(", ", userIds.Select(id => userMap.TryGetValue(id, out var n) ? n : id));
    }

    private async Task GenerateReservationsFromScheduleAsync(Actividad actividad)
    {
        var periodo = await _unitOfWork.Periodos.GetByIdAsync(actividad.PeriodoId);
        if (periodo is null) return;

        if (!Enum.TryParse<DayOfWeek>(actividad.DiaSemana, ignoreCase: true, out var dayOfWeek))
            return;

        var today = DateOnly.FromDateTime(DateTime.Now);
        var userId = actividad.Docentes.FirstOrDefault()?.DocenteId;
        if (string.IsNullOrEmpty(userId))
            userId = _currentUser.UserId.ToString();
        if (!Guid.TryParse(userId, out var userGuid))
            return;

        var startDate = actividad.HoraInicio!.Value;
        var endTime = actividad.HoraFin!.Value;
        var aulaId = actividad.AulaId!.Value;

        var recurringGroupId = Guid.NewGuid();
        var recurrenceRule = JsonSerializer.Serialize(new
        {
            daysOfWeek = new[] { actividad.DiaSemana },
            endDate = periodo.FechaFin.ToString("yyyy-MM-dd")
        });

        for (var date = periodo.FechaInicio; date <= periodo.FechaFin; date = date.AddDays(1))
        {
            if (date < today) continue;
            if (date.DayOfWeek != dayOfWeek) continue;

            if (date.DayOfWeek == DayOfWeek.Sunday) continue;
            if (date.DayOfWeek == DayOfWeek.Saturday && (startDate >= new TimeOnly(16, 0) || endTime > new TimeOnly(16, 0)))
                continue;

            if (await _unitOfWork.Holidays.IsHolidayAsync(date)) continue;

            var reservation = Reservation.Create(
                aulaId, userGuid, actividad.Nombre, null,
                date, startDate, endTime,
                recurringGroupId, recurrenceRule, actividad.Id);

            reservation.Approve(userGuid);
            await _unitOfWork.Reservations.AddAsync(reservation);
        }
    }
}
