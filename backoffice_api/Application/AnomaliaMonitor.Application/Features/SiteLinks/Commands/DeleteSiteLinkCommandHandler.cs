using MediatR;
using AnomaliaMonitor.Domain.Interfaces;

namespace AnomaliaMonitor.Application.Features.SiteLinks.Commands;

public class DeleteSiteLinkCommandHandler : IRequestHandler<DeleteSiteLinkCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSiteLinkCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteSiteLinkCommand request, CancellationToken cancellationToken)
    {
        var siteLink = await _unitOfWork.SiteLinks.GetByIdAsync(request.Id);
        
        if (siteLink == null)
        {
            return false;
        }

        _unitOfWork.SiteLinks.Delete(siteLink);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}