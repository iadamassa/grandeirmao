using MediatR;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SubjectsToResearch.Queries;

public class GetAllSubjectsToResearchQuery : IRequest<PagedResultDto<SubjectToResearchDto>>
{
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}