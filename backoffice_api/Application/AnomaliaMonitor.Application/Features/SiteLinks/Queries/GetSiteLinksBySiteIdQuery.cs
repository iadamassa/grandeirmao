using MediatR;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SiteLinks.Queries;

public class GetSiteLinksBySiteIdQuery : IRequest<IEnumerable<SiteLinkDto>>
{
    public int SiteId { get; set; }
    public bool? IsActive { get; set; }
}