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

    public async Task<T> GetOneByIdAsync(int id, bool tracking = false)
    {
        var query = _context.Set<T>().AsQueryable();
        if (!tracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
    }

    public async Task<List<T>> GetManyByFilterAsync(Expression<Func<T, bool>> filter, int? page, int? pageSize, bool tracking = false, 
        params Expression<Func<T, object>>[] includes)
    {
        var query = _context.Set<T>().AsQueryable();

        if (includes != null)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }

        if (!tracking)
            query = query.AsNoTracking();

        query = query.Where(filter);
        
        if (page.HasValue && pageSize.HasValue)
            return await query
                .Skip(page.Value * pageSize.Value)
                .Take(pageSize.Value)
                .ToListAsync();
        
        return await query.ToListAsync();
    }

    public async Task<T> GetOneByFilterAsync(Expression<Func<T, bool>> filter, bool tracking = false)
    {
        var query = _context.Set<T>().AsQueryable();
        if (!tracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(filter);
    }
}
