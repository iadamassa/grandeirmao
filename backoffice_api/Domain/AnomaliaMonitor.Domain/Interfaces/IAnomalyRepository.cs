using AnomaliaMonitor.Domain.Entities;

namespace AnomaliaMonitor.Domain.Interfaces;

public interface IAnomalyRepository : IGenericRepository<Anomaly>
{
    Task<IEnumerable<Anomaly>> GetAllWithIncludesAsync();
    Task<Anomaly?> GetByIdWithIncludesAsync(int id);
}