using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.Anomalies.Commands;

public class UpdateAnomalyCommandHandler : IRequestHandler<UpdateAnomalyCommand, AnomalyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateAnomalyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AnomalyDto> Handle(UpdateAnomalyCommand request, CancellationToken cancellationToken)
    {
        var anomaly = await _unitOfWork.Anomalies.GetByIdAsync(request.Id);
        
        if (anomaly == null)
        {
            throw new ArgumentException("Anomaly not found");
        }

        anomaly.SiteLinkId = request.SiteLinkId;
        anomaly.SubjectToResearchId = request.SubjectToResearchId;
        anomaly.EstimatedDateTime = request.EstimatedDateTime;
        anomaly.IdentifiedSubject = request.IdentifiedSubject;
        anomaly.ExampleOrReason = request.ExampleOrReason;
        anomaly.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Anomalies.Update(anomaly);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AnomalyDto>(anomaly);
    }
}