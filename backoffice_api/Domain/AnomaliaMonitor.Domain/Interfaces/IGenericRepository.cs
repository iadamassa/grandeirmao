using System.Linq.Expressions;

namespace AnomaliaMonitor.Domain.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
    Task<T> AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();
    Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize);
    Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<int> ids);
    Task<T?> GetByIdWithIncludesAsync(int id, params string[] includeProperties);
    Task<IEnumerable<T>> GetAllWithIncludesAsync(params string[] includeProperties);
    Task<(IEnumerable<T> Data, int TotalCount)> GetPagedWithIncludesAsync(int page, int pageSize, Expression<Func<T, bool>>? filter = null, params string[] includeProperties);
}