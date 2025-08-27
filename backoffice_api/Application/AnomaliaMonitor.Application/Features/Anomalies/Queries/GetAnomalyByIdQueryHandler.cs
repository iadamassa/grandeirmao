using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.Anomalies.Queries;

public class GetAnomalyByIdQueryHandler : IRequestHandler<GetAnomalyByIdQuery, AnomalyDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAnomalyByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AnomalyDto?> Handle(GetAnomalyByIdQuery request, CancellationToken cancellationToken)
    {
        var anomaly = await _unitOfWork.Anomalies.GetByIdWithIncludesAsync(request.Id);
        
        if (anomaly == null)
        {
            return null;
        }

        return _mapper.Map<AnomalyDto>(anomaly);
    }
}