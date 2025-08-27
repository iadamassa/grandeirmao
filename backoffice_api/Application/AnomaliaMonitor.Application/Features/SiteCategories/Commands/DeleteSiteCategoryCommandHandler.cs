using MediatR;
using AnomaliaMonitor.Domain.Interfaces;

namespace AnomaliaMonitor.Application.Features.SiteCategories.Commands;

public class DeleteSiteCategoryCommandHandler : IRequestHandler<DeleteSiteCategoryCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSiteCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteSiteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.SiteCategories.GetByIdAsync(request.Id);
        
        if (category == null)
        {
            return false;
        }

        _unitOfWork.SiteCategories.Delete(category);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}