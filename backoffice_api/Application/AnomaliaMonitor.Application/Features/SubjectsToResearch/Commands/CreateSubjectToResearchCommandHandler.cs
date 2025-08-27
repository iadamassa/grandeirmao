using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Entities;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SubjectsToResearch.Commands;

public class CreateSubjectToResearchCommandHandler : IRequestHandler<CreateSubjectToResearchCommand, SubjectToResearchDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSubjectToResearchCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SubjectToResearchDto> Handle(CreateSubjectToResearchCommand request, CancellationToken cancellationToken)
    {
        var entity = new SubjectToResearch
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        // Add categories if provided
        if (request.CategoryIds != null && request.CategoryIds.Any())
        {
            var categories = await _unitOfWork.SiteCategories.GetByIdsAsync(request.CategoryIds);
            entity.Categories = categories.ToList();
        }

        var createdEntity = await _unitOfWork.SubjectsToResearch.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SubjectToResearchDto>(createdEntity);
    }
}