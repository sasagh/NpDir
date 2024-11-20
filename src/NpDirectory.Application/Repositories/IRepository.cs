using System.Linq.Expressions;

namespace NpDirectory.Application.Repositories;

public interface IRepository<T> where T : class
{
    Task CreateAsync(T entity);

    Task UpdateAsync(T entity);

    Task<bool> DeleteSingleAsync(int id);

    Task<T> GetOneByIdAsync(int id, bool tracking = false);

    Task<List<T>> GetManyByFilterAsync(
        Expression<Func<T, bool>> filter, int? page, int? pageSize, bool tracking = false, params Expression<Func<T, object>>[] includes);

    Task<T> GetOneByFilterAsync(Expression<Func<T, bool>> filter, bool tracking = false);
}