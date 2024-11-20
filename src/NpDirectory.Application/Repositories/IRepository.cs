using System.Linq.Expressions;

namespace NpDirectory.Application.Repositories;

public interface IRepository<T> where T : class
{
    Task CreateAsync(T entity);
    
    Task UpdateAsync(T entity);
    
    Task<bool> DeleteSingleAsync(int id);
    
    Task<T> GetSingleByIdAsync(int id);
    
    Task<T> GetSingleByKeyAsync<TKey>(TKey key, params Expression<Func<T, object>>[] includes);
    
    Task<List<T>> GetAllAsync();
    
    Task<List<T>> GetManyByFilterAsync(
        Expression<Func<T, bool>> filter, int page, int pageSize, params Expression<Func<T, object>>[] includes);
    
    Task<T> GetFirstByFilterAsync(Expression<Func<T, bool>> filter);
}