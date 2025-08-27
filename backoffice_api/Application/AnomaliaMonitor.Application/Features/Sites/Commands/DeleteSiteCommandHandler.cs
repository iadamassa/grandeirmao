using MediatR;
using AnomaliaMonitor.Domain.Interfaces;

namespace AnomaliaMonitor.Application.Features.Sites.Commands;

public class DeleteSiteCommandHandler : IRequestHandler<DeleteSiteCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSiteCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteSiteCommand request, CancellationToken cancellationToken)
    {
        var site = await _unitOfWork.Sites.GetByIdAsync(request.Id);
        
        if (site == null)
        {
            return false;
        }

        _unitOfWork.Sites.Delete(site);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}