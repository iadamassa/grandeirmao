using System.Linq.Expressions;
using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Entities;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.Sites.Queries;

public class GetAllSitesQueryHandler : IRequestHandler<GetAllSitesQuery, PagedResultDto<SiteDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllSitesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<SiteDto>> Handle(GetAllSitesQuery request, CancellationToken cancellationToken)
    {
        // Build filter expression
        Expression<Func<Site, bool>>? filter = null;
        
        if (request.IsActive.HasValue || request.CategoryId.HasValue || !string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            filter = site => 
                (!request.IsActive.HasValue || site.IsActive == request.IsActive.Value) &&
                (!request.CategoryId.HasValue || site.Categories.Any(c => c.Id == request.CategoryId.Value)) &&
                (string.IsNullOrWhiteSpace(request.SearchTerm) || 
                 site.Name.Contains(request.SearchTerm) || 
                 site.Url.Contains(request.SearchTerm));
        }

        var (entities, totalCount) = await _unitOfWork.Sites.GetPagedWithIncludesAsync(
            request.Page, 
            request.PageSize, 
            filter,
            "Categories");

        // Order by name
        var orderedEntities = entities.OrderBy(s => s.Name);
        var mappedEntities = _mapper.Map<IEnumerable<SiteDto>>(orderedEntities);

        return new PagedResultDto<SiteDto>
        {
            Data = mappedEntities,
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize
        };
    }
}