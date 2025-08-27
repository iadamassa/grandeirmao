using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SubjectExamples.Queries;

public class GetSubjectExamplesBySubjectIdQueryHandler : IRequestHandler<GetSubjectExamplesBySubjectIdQuery, IEnumerable<SubjectExampleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSubjectExamplesBySubjectIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SubjectExampleDto>> Handle(GetSubjectExamplesBySubjectIdQuery request, CancellationToken cancellationToken)
    {
        var examples = await _unitOfWork.SubjectExamples.FindAsync(e => e.SubjectToResearchId == request.SubjectToResearchId);

        return _mapper.Map<IEnumerable<SubjectExampleDto>>(examples.OrderBy(e => e.Title));
    }
}