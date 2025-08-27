using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AnomaliaMonitor.Application.Features.Dashboard.Queries;

namespace AnomaliaMonitor.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardData([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var query = new GetDashboardDataQuery { StartDate = startDate, EndDate = endDate };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}