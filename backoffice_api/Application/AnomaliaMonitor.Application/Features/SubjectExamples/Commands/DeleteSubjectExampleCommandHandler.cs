using MediatR;
using AnomaliaMonitor.Domain.Interfaces;

namespace AnomaliaMonitor.Application.Features.SubjectExamples.Commands;

public class DeleteSubjectExampleCommandHandler : IRequestHandler<DeleteSubjectExampleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSubjectExampleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteSubjectExampleCommand request, CancellationToken cancellationToken)
    {
        var example = await _unitOfWork.SubjectExamples.GetByIdAsync(request.Id);
        
        if (example == null)
        {
            return false;
        }

        _unitOfWork.SubjectExamples.Delete(example);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}