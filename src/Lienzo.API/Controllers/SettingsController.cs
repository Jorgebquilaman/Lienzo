using Lienzo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class SettingsController : BaseApiController
{
    private readonly ISystemSettingService _settings;

    public SettingsController(ISystemSettingService settings)
    {
        _settings = settings;
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

    public record SetPublicUrlRequest(string Url);
}
