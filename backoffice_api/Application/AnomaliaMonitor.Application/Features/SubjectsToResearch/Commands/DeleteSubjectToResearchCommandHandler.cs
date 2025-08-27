using MediatR;
using AnomaliaMonitor.Domain.Interfaces;

namespace AnomaliaMonitor.Application.Features.SubjectsToResearch.Commands;

public class DeleteSubjectToResearchCommandHandler : IRequestHandler<DeleteSubjectToResearchCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSubjectToResearchCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteSubjectToResearchCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.SubjectsToResearch.GetByIdAsync(request.Id);
        if (entity == null)
        {
            return false;
        }

        _unitOfWork.SubjectsToResearch.Delete(entity);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}