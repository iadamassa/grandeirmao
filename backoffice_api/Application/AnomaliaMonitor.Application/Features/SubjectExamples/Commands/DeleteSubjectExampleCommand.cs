using MediatR;

namespace AnomaliaMonitor.Application.Features.SubjectExamples.Commands;

public class DeleteSubjectExampleCommand : IRequest<bool>
{
    public int Id { get; set; }
}