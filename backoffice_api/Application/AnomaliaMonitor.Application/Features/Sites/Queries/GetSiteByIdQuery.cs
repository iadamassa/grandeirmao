using MediatR;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.Sites.Queries;

public class GetSiteByIdQuery : IRequest<SiteDto?>
{
    public int Id { get; set; }
}