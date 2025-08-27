using System.Linq.Expressions;
using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Entities;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SubjectsToResearch.Queries;

public class GetAllSubjectsToResearchQueryHandler : IRequestHandler<GetAllSubjectsToResearchQuery, PagedResultDto<SubjectToResearchDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllSubjectsToResearchQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<SubjectToResearchDto>> Handle(GetAllSubjectsToResearchQuery request, CancellationToken cancellationToken)
    {
        // Build filter expression
        Expression<Func<SubjectToResearch, bool>>? filter = null;
        
        if (request.IsActive.HasValue || !string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            filter = subject => 
                (!request.IsActive.HasValue || subject.IsActive == request.IsActive.Value) &&
                (string.IsNullOrWhiteSpace(request.SearchTerm) || 
                 subject.Name.Contains(request.SearchTerm) || 
                 subject.Description.Contains(request.SearchTerm));
        }

        var (entities, totalCount) = await _unitOfWork.SubjectsToResearch.GetPagedWithIncludesAsync(
            request.Page, 
            request.PageSize, 
            filter, 
            "Categories", "Examples");

        var mappedEntities = _mapper.Map<IEnumerable<SubjectToResearchDto>>(entities);

        return new PagedResultDto<SubjectToResearchDto>
        {
            Data = mappedEntities,
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize
        };
    }
}