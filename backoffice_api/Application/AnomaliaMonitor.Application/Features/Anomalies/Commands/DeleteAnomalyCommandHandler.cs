using MediatR;
using AnomaliaMonitor.Domain.Interfaces;

namespace AnomaliaMonitor.Application.Features.Anomalies.Commands;

public class DeleteAnomalyCommandHandler : IRequestHandler<DeleteAnomalyCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAnomalyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteAnomalyCommand request, CancellationToken cancellationToken)
    {
        var anomaly = await _unitOfWork.Anomalies.GetByIdAsync(request.Id);
        
        if (anomaly == null)
        {
            return false;
        }

        _unitOfWork.Anomalies.Delete(anomaly);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}