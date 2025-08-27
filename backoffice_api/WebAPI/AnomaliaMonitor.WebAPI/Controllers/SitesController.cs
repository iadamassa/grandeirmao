using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AnomaliaMonitor.Application.Features.Sites.Commands;
using AnomaliaMonitor.Application.Features.Sites.Queries;
using AnomaliaMonitor.Application.Features.SiteLinks.Commands;
using AnomaliaMonitor.Infrastructure.Services;

namespace AnomaliaMonitor.WebAPI.Controllers;

[ApiController]
[Route("api/sites")]
[Authorize]
public class SitesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IExcelExportService _excelExportService;

    public SitesController(IMediator mediator, IExcelExportService excelExportService)
    {
        _mediator = mediator;
        _excelExportService = excelExportService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null, [FromQuery] int? categoryId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllSitesQuery 
        { 
            IsActive = isActive, 
            CategoryId = categoryId,
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetSiteByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSiteCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSiteCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteSiteCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }

    [HttpPost("{id}/links")]
    public async Task<IActionResult> CreateSiteLink(int id, [FromBody] CreateSiteLinkForSiteRequest request)
    {
        var command = new CreateSiteLinkCommand
        {
            SiteId = id,
            Name = request.Name,
            Url = request.Url,
            Description = request.Description,
            IsActive = request.IsActive
        };
        
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = id }, result);
    }

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportToExcel([FromQuery] bool? isActive = null, [FromQuery] int? categoryId = null)
    {
        var query = new GetAllSitesQuery 
        { 
            IsActive = isActive, 
            CategoryId = categoryId,
            Page = 1,
            PageSize = int.MaxValue // Get all records for export
        };
        var result = await _mediator.Send(query);
        
        var excelData = _excelExportService.ExportSitesToExcel(result.Data);
        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                   $"sites_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }
}

public class CreateSiteLinkForSiteRequest
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}