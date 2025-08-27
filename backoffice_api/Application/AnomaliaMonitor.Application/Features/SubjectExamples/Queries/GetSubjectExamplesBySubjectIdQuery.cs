using MediatR;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SubjectExamples.Queries;

public class GetSubjectExamplesBySubjectIdQuery : IRequest<IEnumerable<SubjectExampleDto>>
{
    public int SubjectToResearchId { get; set; }
}