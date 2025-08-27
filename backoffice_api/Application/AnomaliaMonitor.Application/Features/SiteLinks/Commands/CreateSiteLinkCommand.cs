using MediatR;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SiteLinks.Commands;

public class CreateSiteLinkCommand : IRequest<SiteLinkDto>
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SiteId { get; set; }
    public bool IsActive { get; set; } = true;
}