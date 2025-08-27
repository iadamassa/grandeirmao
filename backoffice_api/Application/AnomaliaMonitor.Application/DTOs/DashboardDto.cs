namespace AnomaliaMonitor.Application.DTOs;

public class DashboardDto
{
    public int TotalSubjects { get; set; }
    public int TotalCategories { get; set; }
    public int TotalSites { get; set; }
    public int TotalAnomalies { get; set; }
    public int TotalLinks { get; set; }
    public int TotalLinksWithAnomalies { get; set; }
    public decimal AnomalyPercentage { get; set; }
    public List<AnomalyChartDto> AnomaliesChart { get; set; } = new();
    public List<TopSiteDto> TopSites { get; set; } = new();
    public List<TopSubjectDto> TopSubjects { get; set; } = new();
}

public class AnomalyChartDto
{
    public DateTimeOffset Date { get; set; }
    public int Count { get; set; }
}

public class TopSiteDto
{
    public string SiteName { get; set; } = string.Empty;
    public int AnomaliesCount { get; set; }
}

public class TopSubjectDto
{
    public string SubjectName { get; set; } = string.Empty;
    public int AnomaliesCount { get; set; }
}