using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AnomaliaMonitor.Application.Features.SubjectExamples.Commands;
using AnomaliaMonitor.Application.Features.SubjectExamples.Queries;

namespace AnomaliaMonitor.WebAPI.Controllers;

[ApiController]
[Route("api/subject-examples")]
[Authorize]
public class SubjectExamplesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubjectExamplesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("by-subject/{subjectId}")]
    public async Task<IActionResult> GetBySubjectId(int subjectId)
    {
        var query = new GetSubjectExamplesBySubjectIdQuery 
        { 
            SubjectToResearchId = subjectId
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubjectExampleCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetBySubjectId), new { subjectId = result.SubjectToResearchId }, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteSubjectExampleCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}