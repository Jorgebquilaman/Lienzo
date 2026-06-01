using Lienzo.Application.Queries.GetCampusStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize]
public class CampusController : BaseApiController
{
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var query = new GetCampusStatusQuery();
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
}
