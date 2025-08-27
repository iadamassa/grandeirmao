using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SiteLinks.Queries;

public class GetSiteLinksBySiteIdQueryHandler : IRequestHandler<GetSiteLinksBySiteIdQuery, IEnumerable<SiteLinkDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSiteLinksBySiteIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SiteLinkDto>> Handle(GetSiteLinksBySiteIdQuery request, CancellationToken cancellationToken)
    {
        var siteLinks = await _unitOfWork.SiteLinks.FindAsync(sl => sl.SiteId == request.SiteId);

        if (request.IsActive.HasValue)
        {
            siteLinks = siteLinks.Where(sl => sl.IsActive == request.IsActive.Value);
        }

        return _mapper.Map<IEnumerable<SiteLinkDto>>(siteLinks.OrderBy(sl => sl.Name));
    }
}