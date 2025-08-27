using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Entities;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;
using AnomaliaMonitor.Application.Services;

namespace AnomaliaMonitor.Application.Features.Sites.Commands;

public class CreateSiteCommandHandler : IRequestHandler<CreateSiteCommand, SiteDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICrawlRequestService _crawlRequestService;

    public CreateSiteCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICrawlRequestService crawlRequestService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _crawlRequestService = crawlRequestService;
    }

    public async Task<SiteDto> Handle(CreateSiteCommand request, CancellationToken cancellationToken)
    {
        var site = new Site
        {
            Name = request.Name,
            Url = request.Url,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        // Add categories
        if (request.CategoryIds.Any())
        {
            var categories = await _unitOfWork.SiteCategories.GetByIdsAsync(request.CategoryIds);
            site.Categories = categories.ToList();
        }

        await _unitOfWork.Sites.AddAsync(site);
        await _unitOfWork.SaveChangesAsync();

        // Publish crawl request message for the new site
        if (site.IsActive)
        {
            await _crawlRequestService.PublishCrawlRequestAsync(site);
        }

        return _mapper.Map<SiteDto>(site);
    }
}