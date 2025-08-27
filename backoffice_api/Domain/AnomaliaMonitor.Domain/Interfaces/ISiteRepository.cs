using AnomaliaMonitor.Domain.Entities;

namespace AnomaliaMonitor.Domain.Interfaces;

public interface ISiteRepository : IGenericRepository<Site>
{
    Task<Site?> GetByIdWithCategoriesAsync(int id);
    Task<IEnumerable<Site>> GetAllWithCategoriesAsync();
}