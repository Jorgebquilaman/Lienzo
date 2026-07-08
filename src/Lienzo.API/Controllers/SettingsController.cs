using Lienzo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class SettingsController : BaseApiController
{
    private readonly ISystemSettingService _settings;
    private readonly IEmailService _emailService;

    public SettingsController(ISystemSettingService settings, IEmailService emailService)
    {
        _settings = settings;
        _emailService = emailService;
    }

    [HttpGet("public-url")]
    public async Task<IActionResult> GetPublicUrl()
    {
        var url = await _settings.GetValueAsync("PublicUrl");
        return Ok(new { url = url ?? "" });
    }

    [HttpPut("public-url")]
    public async Task<IActionResult> SetPublicUrl([FromBody] SetPublicUrlRequest request)
    {
        await _settings.SetValueAsync("PublicUrl", request.Url);
        return Ok(new { url = request.Url });
    }

    [HttpGet("email")]
    public async Task<IActionResult> GetEmailSettings()
    {
        var settings = new
        {
            smtpHost = await _settings.GetValueAsync("EmailSmtpHost") ?? "",
            smtpPort = await _settings.GetValueAsync("EmailSmtpPort") ?? "",
            username = await _settings.GetValueAsync("EmailUsername") ?? "",
            password = await _settings.GetValueAsync("EmailPassword") ?? "",
            fromAddress = await _settings.GetValueAsync("EmailFromAddress") ?? "",
            fromName = await _settings.GetValueAsync("EmailFromName") ?? "",
        };
        return Ok(settings);
    }

    [HttpPut("email")]
    public async Task<IActionResult> SetEmailSettings([FromBody] EmailSettingsRequest request)
    {
        await _settings.SetValueAsync("EmailSmtpHost", request.SmtpHost);
        await _settings.SetValueAsync("EmailSmtpPort", request.SmtpPort);
        await _settings.SetValueAsync("EmailUsername", request.Username);
        await _settings.SetValueAsync("EmailPassword", request.Password);
        await _settings.SetValueAsync("EmailFromAddress", request.FromAddress);
        await _settings.SetValueAsync("EmailFromName", request.FromName);
        return Ok(new { message = "Configuración de email guardada correctamente" });
    }

    [HttpPost("email/test")]
    public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest request)
    {
        try
        {
            await _emailService.SendAsync(request.To, "Prueba de configuración de email - Lienzo",
                "<h1>Prueba de email</h1><p>Si recibís este mensaje, la configuración de email es correcta.</p>");
            return Ok(new { message = "Email de prueba enviado correctamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error al enviar email de prueba: {ex.Message}" });
        }
    }

    public record SetPublicUrlRequest(string Url);
    public record EmailSettingsRequest(string SmtpHost, string SmtpPort, string Username, string Password, string FromAddress, string FromName);
    public record TestEmailRequest(string To);
}
