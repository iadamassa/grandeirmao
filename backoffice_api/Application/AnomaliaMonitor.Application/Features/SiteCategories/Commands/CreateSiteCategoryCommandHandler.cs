using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Entities;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SiteCategories.Commands;

public class CreateSiteCategoryCommandHandler : IRequestHandler<CreateSiteCategoryCommand, SiteCategoryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSiteCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SiteCategoryDto> Handle(CreateSiteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new SiteCategory
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SiteCategories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SiteCategoryDto>(category);
    }
}