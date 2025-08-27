using AnomaliaMonitor.Domain.Entities;

namespace AnomaliaMonitor.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<SubjectToResearch> SubjectsToResearch { get; }
    IGenericRepository<SubjectExample> SubjectExamples { get; }
    IGenericRepository<SiteCategory> SiteCategories { get; }
    ISiteRepository Sites { get; }
    IGenericRepository<SiteLink> SiteLinks { get; }
    IAnomalyRepository Anomalies { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}