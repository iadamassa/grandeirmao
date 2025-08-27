using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AnomaliaMonitor.Application.Features.SiteCategories.Commands;
using AnomaliaMonitor.Application.Features.SiteCategories.Queries;
using AnomaliaMonitor.Infrastructure.Services;

namespace AnomaliaMonitor.WebAPI.Controllers;

[ApiController]
[Route("api/site-categories")]
[Authorize]
public class SiteCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IExcelExportService _excelExportService;

    public SiteCategoriesController(IMediator mediator, IExcelExportService excelExportService)
    {
        _mediator = mediator;
        _excelExportService = excelExportService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllSiteCategoriesQuery 
        { 
            IsActive = isActive,
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
        var query = new GetSiteCategoryByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSiteCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSiteCategoryCommand command)
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
        var command = new DeleteSiteCategoryCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportToExcel([FromQuery] bool? isActive = null)
    {
        var query = new GetAllSiteCategoriesQuery 
        { 
            IsActive = isActive,
            Page = 1,
            PageSize = int.MaxValue // Get all records for export
        };
        var result = await _mediator.Send(query);
        
        var excelData = _excelExportService.ExportSiteCategoriesToExcel(result.Data);
        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                   $"categorias_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }
}