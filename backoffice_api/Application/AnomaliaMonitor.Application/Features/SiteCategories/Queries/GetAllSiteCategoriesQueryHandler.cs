using System.Linq.Expressions;
using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Entities;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SiteCategories.Queries;

public class GetAllSiteCategoriesQueryHandler : IRequestHandler<GetAllSiteCategoriesQuery, PagedResultDto<SiteCategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllSiteCategoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<SiteCategoryDto>> Handle(GetAllSiteCategoriesQuery request, CancellationToken cancellationToken)
    {
        // Build filter expression
        Expression<Func<SiteCategory, bool>>? filter = null;
        
        if (request.IsActive.HasValue || !string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            filter = category => 
                (!request.IsActive.HasValue || category.IsActive == request.IsActive.Value) &&
                (string.IsNullOrWhiteSpace(request.SearchTerm) || 
                 category.Name.Contains(request.SearchTerm) || 
                 category.Description.Contains(request.SearchTerm));
        }

        var (entities, totalCount) = await _unitOfWork.SiteCategories.GetPagedWithIncludesAsync(
            request.Page, 
            request.PageSize, 
            filter);

        // Order by name
        var orderedEntities = entities.OrderBy(c => c.Name);
        var mappedEntities = _mapper.Map<IEnumerable<SiteCategoryDto>>(orderedEntities);

        return new PagedResultDto<SiteCategoryDto>
        {
            Data = mappedEntities,
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize
        };
    }
}