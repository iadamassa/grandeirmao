using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SiteCategories.Queries;

public class GetSiteCategoryByIdQueryHandler : IRequestHandler<GetSiteCategoryByIdQuery, SiteCategoryDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSiteCategoryByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SiteCategoryDto?> Handle(GetSiteCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.SiteCategories.GetByIdAsync(request.Id);
        
        if (category == null)
        {
            return null;
        }

        return _mapper.Map<SiteCategoryDto>(category);
    }
}