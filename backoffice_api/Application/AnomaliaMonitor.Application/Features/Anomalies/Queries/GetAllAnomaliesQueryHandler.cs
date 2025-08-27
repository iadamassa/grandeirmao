using System.Linq.Expressions;
using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Entities;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.Anomalies.Queries;

public class GetAllAnomaliesQueryHandler : IRequestHandler<GetAllAnomaliesQuery, PagedResultDto<AnomalyDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllAnomaliesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<AnomalyDto>> Handle(GetAllAnomaliesQuery request, CancellationToken cancellationToken)
    {
        // Build filter expression
        Expression<Func<Anomaly, bool>>? filter = null;
        
        if (request.StartDate.HasValue || request.EndDate.HasValue || request.SubjectToResearchId.HasValue || !string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            filter = anomaly => 
                (!request.StartDate.HasValue || anomaly.EstimatedDateTime >= request.StartDate.Value) &&
                (!request.EndDate.HasValue || anomaly.EstimatedDateTime <= request.EndDate.Value) &&
                (!request.SubjectToResearchId.HasValue || anomaly.SubjectToResearchId == request.SubjectToResearchId.Value) &&
                (string.IsNullOrWhiteSpace(request.SearchTerm) || 
                 anomaly.IdentifiedSubject.Contains(request.SearchTerm) || 
                 anomaly.ExampleOrReason.Contains(request.SearchTerm));
        }

        var (entities, totalCount) = await _unitOfWork.Anomalies.GetPagedWithIncludesAsync(
            request.Page, 
            request.PageSize, 
            filter,
            "SubjectToResearch", "SiteLink", "SiteLink.Site");

        // Order by estimated datetime descending
        var orderedEntities = entities.OrderByDescending(a => a.EstimatedDateTime);
        var mappedEntities = _mapper.Map<IEnumerable<AnomalyDto>>(orderedEntities);

        return new PagedResultDto<AnomalyDto>
        {
            Data = mappedEntities,
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize
        };
    }
}