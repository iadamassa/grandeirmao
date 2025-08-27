using MediatR;

namespace AnomaliaMonitor.Application.Features.SiteLinks.Commands;

public class DeleteSiteLinkCommand : IRequest<bool>
{
    public int Id { get; set; }
}