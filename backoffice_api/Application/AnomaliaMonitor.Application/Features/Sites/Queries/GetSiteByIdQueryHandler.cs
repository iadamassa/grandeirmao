using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.Sites.Queries;

public class GetSiteByIdQueryHandler : IRequestHandler<GetSiteByIdQuery, SiteDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSiteByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SiteDto?> Handle(GetSiteByIdQuery request, CancellationToken cancellationToken)
    {
        var site = await _unitOfWork.Sites.GetByIdWithCategoriesAsync(request.Id);
        
        if (site == null)
        {
            return null;
        }

        return _mapper.Map<SiteDto>(site);
    }
}