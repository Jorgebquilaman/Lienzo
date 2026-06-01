using Lienzo.Application.Commands.CreateHoliday;
using Lienzo.Application.Commands.DeleteHoliday;
using Lienzo.Application.DTOs;
using Lienzo.Application.Queries.GetAllHolidays;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class HolidaysController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllHolidaysQuery());
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHolidayRequest request)
    {
        var result = await Mediator.Send(new CreateHolidayCommand(request));
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteHolidayCommand(id));
        return HandleResult(result);
    }
}
