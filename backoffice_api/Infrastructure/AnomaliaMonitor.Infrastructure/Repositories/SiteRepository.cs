using Microsoft.EntityFrameworkCore;
using AnomaliaMonitor.Domain.Entities;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Infrastructure.Data;

namespace AnomaliaMonitor.Infrastructure.Repositories;

public class SiteRepository : GenericRepository<Site>, ISiteRepository
{
    public SiteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Site?> GetByIdWithCategoriesAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Categories)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Site>> GetAllWithCategoriesAsync()
    {
        return await _dbSet
            .Include(s => s.Categories)
            .ToListAsync();
    }
}