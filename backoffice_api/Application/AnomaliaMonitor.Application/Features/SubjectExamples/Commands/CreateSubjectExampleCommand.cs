using MediatR;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SubjectExamples.Commands;

public class CreateSubjectExampleCommand : IRequest<SubjectExampleDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SubjectToResearchId { get; set; }
}