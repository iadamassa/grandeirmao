using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Entities;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SiteLinks.Commands;

public class CreateSiteLinkCommandHandler : IRequestHandler<CreateSiteLinkCommand, SiteLinkDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSiteLinkCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SiteLinkDto> Handle(CreateSiteLinkCommand request, CancellationToken cancellationToken)
    {
        var siteLink = new SiteLink
        {
            Name = request.Name,
            Url = request.Url,
            Description = request.Description,
            SiteId = request.SiteId,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SiteLinks.AddAsync(siteLink);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SiteLinkDto>(siteLink);
    }
}