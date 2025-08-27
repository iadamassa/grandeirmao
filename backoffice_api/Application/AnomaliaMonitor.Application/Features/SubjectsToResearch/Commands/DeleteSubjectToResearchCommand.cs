using MediatR;

namespace AnomaliaMonitor.Application.Features.SubjectsToResearch.Commands;

public class DeleteSubjectToResearchCommand : IRequest<bool>
{
    public int Id { get; set; }
}