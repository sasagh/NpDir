using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NpDirectory.Application.Repositories;

namespace NpDirectory.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public Task CreateAsync(T entity)
    {
        _context.Set<T>().Add(entity);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);

        return Task.CompletedTask;
    }

    public async Task<bool> DeleteSingleAsync(int id)
    {
        var entity = await _context.Set<T>().FindAsync(id);
        if (entity == null)
            return false;

        _context.Set<T>().Remove(entity);

        return true;
    }

    public Task<T> GetSingleByIdAsync(int id)
        => _context.Set<T>().FindAsync(id).AsTask();

    public async Task<T> GetSingleByKeyAsync<TKey>(TKey key, params Expression<Func<T, object>>[] includes)
    {
        var query = _context.Set<T>().AsQueryable();
        foreach (var include in includes)
            query = query.Include(include);

        return await query.FirstOrDefaultAsync(e => e.Equals(key));
    }
    
    public Task<List<T>> GetAllAsync()
        => _context.Set<T>().ToListAsync();
    
    public async Task<List<T>> GetManyByFilterAsync(Expression<Func<T, bool>> filter, int page, int pageSize, 
        params Expression<Func<T, object>>[] includes)
    {
        var query = _context.Set<T>().AsQueryable();
   
        if (includes != null)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }

        return await query
            .Where(filter)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public Task<T> GetFirstByFilterAsync(Expression<Func<T, bool>> filter)
        => _context.Set<T>().FirstOrDefaultAsync(filter);
}