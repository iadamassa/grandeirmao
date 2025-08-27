using Microsoft.EntityFrameworkCore.Storage;
using AnomaliaMonitor.Domain.Entities;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Infrastructure.Data;

namespace AnomaliaMonitor.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        SubjectsToResearch = new GenericRepository<SubjectToResearch>(_context);
        SubjectExamples = new GenericRepository<SubjectExample>(_context);
        SiteCategories = new GenericRepository<SiteCategory>(_context);
        Sites = new SiteRepository(_context);
        SiteLinks = new GenericRepository<SiteLink>(_context);
        Anomalies = new AnomalyRepository(_context);
    }

    public IGenericRepository<SubjectToResearch> SubjectsToResearch { get; }
    public IGenericRepository<SubjectExample> SubjectExamples { get; }
    public IGenericRepository<SiteCategory> SiteCategories { get; }
    public ISiteRepository Sites { get; }
    public IGenericRepository<SiteLink> SiteLinks { get; }
    public IAnomalyRepository Anomalies { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}