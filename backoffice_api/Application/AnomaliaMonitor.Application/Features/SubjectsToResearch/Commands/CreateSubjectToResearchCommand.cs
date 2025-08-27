using MediatR;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SubjectsToResearch.Commands;

public class CreateSubjectToResearchCommand : IRequest<SubjectToResearchDto>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<int> CategoryIds { get; set; } = new();
}