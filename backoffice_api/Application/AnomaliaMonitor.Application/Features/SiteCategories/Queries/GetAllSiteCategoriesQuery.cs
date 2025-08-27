using MediatR;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SiteCategories.Queries;

public class GetAllSiteCategoriesQuery : IRequest<PagedResultDto<SiteCategoryDto>>
{
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}