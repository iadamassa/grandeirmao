using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AnomaliaMonitor.Application.Features.SiteLinks.Commands;
using AnomaliaMonitor.Application.Features.SiteLinks.Queries;

namespace AnomaliaMonitor.WebAPI.Controllers;

[ApiController]
[Route("api/site-links")]
[Authorize]
public class SiteLinksController : ControllerBase
{
    private readonly IMediator _mediator;

    public SiteLinksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("by-site/{siteId}")]
    public async Task<IActionResult> GetBySiteId(int siteId, [FromQuery] bool? isActive = null)
    {
        var query = new GetSiteLinksBySiteIdQuery 
        { 
            SiteId = siteId, 
            IsActive = isActive 
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSiteLinkCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetBySiteId), new { siteId = result.SiteId }, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteSiteLinkCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}