using Microsoft.EntityFrameworkCore;
using AnomaliaMonitor.Domain.Entities;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Infrastructure.Data;

namespace AnomaliaMonitor.Infrastructure.Repositories;

public class AnomalyRepository : GenericRepository<Anomaly>, IAnomalyRepository
{
    public AnomalyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Anomaly>> GetAllWithIncludesAsync()
    {
        return await _dbSet
            .Include(a => a.SiteLink)
                .ThenInclude(sl => sl.Site)
            .Include(a => a.SubjectToResearch)
            .ToListAsync();
    }

    public async Task<Anomaly?> GetByIdWithIncludesAsync(int id)
    {
        return await _dbSet
            .Include(a => a.SiteLink)
                .ThenInclude(sl => sl.Site)
            .Include(a => a.SubjectToResearch)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
}