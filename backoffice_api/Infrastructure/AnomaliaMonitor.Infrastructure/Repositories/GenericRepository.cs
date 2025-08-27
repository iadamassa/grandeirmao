using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Infrastructure.Data;

namespace AnomaliaMonitor.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
    {
        return await _dbSet.Where(expression).ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.FindAsync(id) != null;
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize)
    {
        return await _dbSet.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _dbSet.Where(e => ids.Contains((int)EF.Property<object>(e, "Id"))).ToListAsync();
    }

    public async Task<T?> GetByIdWithIncludesAsync(int id, params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet;
        
        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }
        
        return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
    }

    public async Task<IEnumerable<T>> GetAllWithIncludesAsync(params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet;
        
        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }
        
        return await query.ToListAsync();
    }

    public async Task<(IEnumerable<T> Data, int TotalCount)> GetPagedWithIncludesAsync(int page, int pageSize, Expression<Func<T, bool>>? filter = null, params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet;
        
        // Apply includes
        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }
        
        // Apply filter if provided
        if (filter != null)
        {
            query = query.Where(filter);
        }
        
        // Get total count before pagination
        var totalCount = await query.CountAsync();
        
        // Apply pagination
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (data, totalCount);
    }
}