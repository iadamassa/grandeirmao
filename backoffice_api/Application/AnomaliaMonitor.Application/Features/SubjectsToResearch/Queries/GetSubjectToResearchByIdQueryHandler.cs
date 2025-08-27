using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SubjectsToResearch.Queries;

public class GetSubjectToResearchByIdQueryHandler : IRequestHandler<GetSubjectToResearchByIdQuery, SubjectToResearchDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSubjectToResearchByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SubjectToResearchDto?> Handle(GetSubjectToResearchByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.SubjectsToResearch.GetByIdWithIncludesAsync(request.Id, "Categories", "Examples");
        return entity == null ? null : _mapper.Map<SubjectToResearchDto>(entity);
    }
}