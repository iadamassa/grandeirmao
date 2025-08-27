using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Entities;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.Anomalies.Commands;

public class CreateAnomalyCommandHandler : IRequestHandler<CreateAnomalyCommand, AnomalyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateAnomalyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AnomalyDto> Handle(CreateAnomalyCommand request, CancellationToken cancellationToken)
    {
        var anomaly = new Anomaly
        {
            SiteLinkId = request.SiteLinkId,
            SubjectToResearchId = request.SubjectToResearchId,
            EstimatedDateTime = request.EstimatedDateTime,
            IdentifiedSubject = request.IdentifiedSubject,
            ExampleOrReason = request.ExampleOrReason,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Anomalies.AddAsync(anomaly);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AnomalyDto>(anomaly);
    }
}