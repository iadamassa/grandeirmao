using MediatR;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.Sites.Queries;

public class GetAllSitesQuery : IRequest<PagedResultDto<SiteDto>>
{
    public bool? IsActive { get; set; }
    public int? CategoryId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}