using MediatR;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.Dashboard.Queries;

public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, DashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardDataQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardDto> Handle(GetDashboardDataQuery request, CancellationToken cancellationToken)
    {
        var totalSubjects = await _unitOfWork.SubjectsToResearch.CountAsync();
        var totalCategories = await _unitOfWork.SiteCategories.CountAsync();
        var totalSites = await _unitOfWork.Sites.CountAsync();
        var totalAnomalies = await _unitOfWork.Anomalies.CountAsync();
        
        // Calculate link statistics
        var allSiteLinks = await _unitOfWork.SiteLinks.GetAllAsync();
        var totalLinks = allSiteLinks.Count();
        var totalLinksWithAnomalies = allSiteLinks.Count(sl => sl.HasAnomaly);
        var anomalyPercentage = totalLinks > 0 ? Math.Round((decimal)totalLinksWithAnomalies / totalLinks * 100, 2) : 0;

        var anomalies = await _unitOfWork.Anomalies.GetAllAsync();
        
        if (request.StartDate.HasValue)
        {
            anomalies = anomalies.Where(a => a.EstimatedDateTime >= request.StartDate.Value);
        }
        
        if (request.EndDate.HasValue)
        {
            anomalies = anomalies.Where(a => a.EstimatedDateTime <= request.EndDate.Value);
        }

        var anomaliesChart = anomalies
            .GroupBy(a => a.EstimatedDateTime.Date)
            .Select(g => new AnomalyChartDto
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToList();

        // Top 10 sites with most anomalies
        var sites = await _unitOfWork.Sites.GetAllAsync();
        var siteLinks = await _unitOfWork.SiteLinks.GetAllAsync();
        
        var topSites = anomalies
            .Join(siteLinks, a => a.SiteLinkId, sl => sl.Id, (a, sl) => new { Anomaly = a, SiteLink = sl })
            .Join(sites, x => x.SiteLink.SiteId, s => s.Id, (x, s) => new { x.Anomaly, Site = s })
            .GroupBy(x => x.Site.Name)
            .Select(g => new TopSiteDto
            {
                SiteName = g.Key,
                AnomaliesCount = g.Count()
            })
            .OrderByDescending(x => x.AnomaliesCount)
            .Take(10)
            .ToList();

        // Top 10 subjects most found
        var subjects = await _unitOfWork.SubjectsToResearch.GetAllAsync();
        
        var topSubjects = anomalies
            .Join(subjects, a => a.SubjectToResearchId, s => s.Id, (a, s) => new { Anomaly = a, Subject = s })
            .GroupBy(x => x.Subject.Name)
            .Select(g => new TopSubjectDto
            {
                SubjectName = g.Key,
                AnomaliesCount = g.Count()
            })
            .OrderByDescending(x => x.AnomaliesCount)
            .Take(10)
            .ToList();

        return new DashboardDto
        {
            TotalSubjects = totalSubjects,
            TotalCategories = totalCategories,
            TotalSites = totalSites,
            TotalAnomalies = totalAnomalies,
            TotalLinks = totalLinks,
            TotalLinksWithAnomalies = totalLinksWithAnomalies,
            AnomalyPercentage = anomalyPercentage,
            AnomaliesChart = anomaliesChart,
            TopSites = topSites,
            TopSubjects = topSubjects
        };
    }
}