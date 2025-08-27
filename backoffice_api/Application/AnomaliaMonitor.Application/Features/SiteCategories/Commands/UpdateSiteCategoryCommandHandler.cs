using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SiteCategories.Commands;

public class UpdateSiteCategoryCommandHandler : IRequestHandler<UpdateSiteCategoryCommand, SiteCategoryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateSiteCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SiteCategoryDto> Handle(UpdateSiteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.SiteCategories.GetByIdAsync(request.Id);
        
        if (category == null)
        {
            throw new ArgumentException("Category not found");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.SiteCategories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SiteCategoryDto>(category);
    }
}