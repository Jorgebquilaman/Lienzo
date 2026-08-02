using Lienzo.Application.Commands.CreateReservationFromEmail;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Application.Queries.GetReservationByEmailUid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class EmailsController : BaseApiController
{
    private readonly IEmailReaderService _emailReader;
    private readonly IEmailEvidenceStorage _evidenceStorage;

    public EmailsController(IEmailReaderService emailReader, IEmailEvidenceStorage evidenceStorage)
    {
        _emailReader = emailReader;
        _evidenceStorage = evidenceStorage;
    }

    [HttpGet]
    public async Task<IActionResult> GetInbox([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _emailReader.GetInboxAsync(page, pageSize);
        return HandleResult(result);
    }

    [HttpGet("{uid}")]
    public async Task<IActionResult> GetMessage(string uid)
    {
        var result = await _emailReader.GetMessageAsync(uid);
        return HandleResult(result);
    }

    [HttpGet("{uid}/download")]
    public async Task<IActionResult> DownloadRaw(string uid)
    {
        var result = await _emailReader.DownloadRawAsync(uid);
        if (result.IsFailure)
            return HandleResult(result);
        return File(result.Value, "message/rfc822", $"{uid}.eml");
    }

    [HttpGet("processed")]
    public async Task<IActionResult> GetProcessed()
    {
        var result = await _emailReader.GetProcessedEmailsAsync();
        return HandleResult(result);
    }

    [HttpGet("{uid}/evidence")]
    public async Task<IActionResult> DownloadEvidence(string uid)
    {
        var reservation = await Mediator.Send(new GetReservationByEmailUidQuery(uid));
        if (reservation.IsFailure || string.IsNullOrEmpty(reservation.Value.EvidenceFilePath))
            return NotFound(new ProblemDetails { Title = "No se encontró evidencia para este correo.", Status = 404 });

        var bytes = await _evidenceStorage.ReadAsync(reservation.Value.EvidenceFilePath);
        if (bytes is null)
            return NotFound(new ProblemDetails { Title = "No se encontró evidencia para este correo.", Status = 404 });

        return File(bytes, "message/rfc822", $"{uid}.eml");
    }

    [HttpPost("{uid}/reservation")]
    public async Task<IActionResult> CreateReservation(string uid, [FromBody] CreateReservationFromEmailRequest request)
    {
        if (request.EmailUid != uid)
            return BadRequest(new ProblemDetails { Title = "El UID del correo no coincide.", Status = 400 });

        var result = await Mediator.Send(new CreateReservationFromEmailCommand(request));
        return HandleResult(result);
    }
}
