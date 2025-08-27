using MediatR;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SiteCategories.Queries;

public class GetSiteCategoryByIdQuery : IRequest<SiteCategoryDto?>
{
    public int Id { get; set; }
}