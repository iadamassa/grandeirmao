using MediatR;

namespace AnomaliaMonitor.Application.Features.SiteCategories.Commands;

public class DeleteSiteCategoryCommand : IRequest<bool>
{
    public int Id { get; set; }
}