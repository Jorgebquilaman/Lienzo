using Lienzo.Application.Commands.CerrarClase;
using Lienzo.Application.Commands.CheckIn;
using Lienzo.Application.Commands.MarcarAsistencia;
using Lienzo.Application.Commands.SyncMissingUsers;
using Lienzo.Application.Commands.SyncSga;
using Lienzo.Application.Commands.ToggleAsistencia;
using Lienzo.Application.Interfaces;
using Lienzo.Application.Queries.GetClase;
using Lienzo.Application.Queries.GetClasePorReserva;
using Lienzo.Application.Queries.GetClasesList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize]
public class AsistenciaController : BaseApiController
{
    private readonly ISystemSettingService _settings;

    public AsistenciaController(ISystemSettingService settings)
    {
        _settings = settings;
    }
    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
    {
        var result = await Mediator.Send(new CheckInCommand(request.ReservationId));
        return HandleResult(result);
    }

    [HttpGet("{claseId:guid}")]
    public async Task<IActionResult> GetClase(Guid claseId)
    {
        var result = await Mediator.Send(new GetClaseQuery(claseId));
        return HandleResult(result);
    }

    [HttpPost("marcar")]
    public async Task<IActionResult> MarcarAsistencia([FromBody] MarcarAsistenciaRequest request)
    {
        var result = await Mediator.Send(new MarcarAsistenciaCommand(request.ClaseId));
        return HandleResult(result);
    }

    [HttpPost("toggle-alumno")]
    public async Task<IActionResult> ToggleAlumno([FromBody] ToggleAlumnoRequest request)
    {
        var result = await Mediator.Send(new ToggleAsistenciaCommand(request.ClaseId, request.AsistenciaId));
        return HandleResult(result);
    }

    [HttpPost("sync-sga/{claseId:guid}")]
    public async Task<IActionResult> SyncSga(Guid claseId)
    {
        var result = await Mediator.Send(new SyncSgaCommand(claseId));
        return HandleResult(result);
    }

    [HttpPost("cerrar/{claseId:guid}")]
    public async Task<IActionResult> CerrarClase(Guid claseId)
    {
        var result = await Mediator.Send(new CerrarClaseCommand(claseId));
        return HandleResult(result);
    }

    [HttpGet("por-reserva/{reservationId:guid}")]
    public async Task<IActionResult> GetClasePorReserva(Guid reservationId)
    {
        var result = await Mediator.Send(new GetClasePorReservaQuery(reservationId));
        return HandleResult(result);
    }

    [HttpGet("qr/{claseId:guid}")]
    public async Task<IActionResult> GetQrUrl(Guid claseId)
    {
        var publicUrl = await _settings.GetValueAsync("PublicUrl");
        var baseUrl = !string.IsNullOrEmpty(publicUrl)
            ? publicUrl.TrimEnd('/')
            : $"{Request.Scheme}://{Request.Host}";
        var url = $"{baseUrl}/asistencia/marcar?claseId={claseId}";
        return Ok(new { url });
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        [FromQuery] Guid? actividadId,
        [FromQuery] string? estado,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetClasesListQuery(desde, hasta, actividadId, estado, page, pageSize));
        return HandleResult(result);
    }

    [HttpPost("{claseId:guid}/sync-missing-users")]
    public async Task<IActionResult> SyncMissingUsers(Guid claseId)
    {
        var result = await Mediator.Send(new SyncMissingUsersCommand(claseId));
        return HandleResult(result);
    }
}

public record CheckInRequest(Guid ReservationId);
public record MarcarAsistenciaRequest(Guid ClaseId);
public record ToggleAlumnoRequest(Guid ClaseId, Guid AsistenciaId);
