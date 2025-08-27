using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SubjectsToResearch.Commands;

public class UpdateSubjectToResearchCommandHandler : IRequestHandler<UpdateSubjectToResearchCommand, SubjectToResearchDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateSubjectToResearchCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SubjectToResearchDto> Handle(UpdateSubjectToResearchCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.SubjectsToResearch.GetByIdWithIncludesAsync(request.Id, "Categories");
        if (entity == null)
        {
            throw new ArgumentException($"Subject with ID {request.Id} not found");
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        // Update categories
        if (request.CategoryIds != null && request.CategoryIds.Any())
        {
            var categories = await _unitOfWork.SiteCategories.GetByIdsAsync(request.CategoryIds);
            entity.Categories = categories.ToList();
        }
        else
        {
            entity.Categories.Clear();
        }

        _unitOfWork.SubjectsToResearch.Update(entity);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SubjectToResearchDto>(entity);
    }
}