using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;
using AnomaliaMonitor.Application.Services;

namespace AnomaliaMonitor.Application.Features.Sites.Commands;

public class UpdateSiteCommandHandler : IRequestHandler<UpdateSiteCommand, SiteDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICrawlRequestService _crawlRequestService;

    public UpdateSiteCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICrawlRequestService crawlRequestService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _crawlRequestService = crawlRequestService;
    }

    public async Task<SiteDto> Handle(UpdateSiteCommand request, CancellationToken cancellationToken)
    {
        var site = await _unitOfWork.Sites.GetByIdWithCategoriesAsync(request.Id);
        
        if (site == null)
        {
            throw new ArgumentException("Site not found");
        }

        site.Name = request.Name;
        site.Url = request.Url;
        site.Description = request.Description;
        site.IsActive = request.IsActive;
        site.UpdatedAt = DateTime.UtcNow;

        // Update categories
        site.Categories.Clear();
        if (request.CategoryIds.Any())
        {
            var categories = await _unitOfWork.SiteCategories.GetByIdsAsync(request.CategoryIds);
            site.Categories = categories.ToList();
        }

        _unitOfWork.Sites.Update(site);
        await _unitOfWork.SaveChangesAsync();

        // Publish crawl request message for the updated site
        if (site.IsActive)
        {
            await _crawlRequestService.PublishCrawlRequestAsync(site);
        }

        return _mapper.Map<SiteDto>(site);
    }
}