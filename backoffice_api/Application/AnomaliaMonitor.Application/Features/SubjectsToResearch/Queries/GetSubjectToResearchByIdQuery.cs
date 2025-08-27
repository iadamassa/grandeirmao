using MediatR;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SubjectsToResearch.Queries;

public class GetSubjectToResearchByIdQuery : IRequest<SubjectToResearchDto?>
{
    public int Id { get; set; }
}