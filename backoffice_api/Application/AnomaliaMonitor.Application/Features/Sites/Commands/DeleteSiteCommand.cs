using MediatR;

namespace AnomaliaMonitor.Application.Features.Sites.Commands;

public class DeleteSiteCommand : IRequest<bool>
{
    public int Id { get; set; }
}