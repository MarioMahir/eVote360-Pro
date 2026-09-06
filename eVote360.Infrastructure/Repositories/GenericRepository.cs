using System.Linq.Expressions;
using eVote360.Core.Interfaces.Repositories;
using eVote360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> Set;

    public GenericRepository(AppDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public Task<T?> GetByIdAsync(int id) => Set.FindAsync(id).AsTask();

    public Task<List<T>> GetAllAsync() => Set.ToListAsync();

    public Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate) => Set.Where(predicate).ToListAsync();

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) => Set.FirstOrDefaultAsync(predicate);

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => Set.AnyAsync(predicate);

    public Task<int> CountAsync(Expression<Func<T, bool>> predicate) => Set.CountAsync(predicate);

    public IQueryable<T> Query() => Set.AsQueryable();

    public Task AddAsync(T entity) => Set.AddAsync(entity).AsTask();

    public Task AddRangeAsync(IEnumerable<T> entities) => Set.AddRangeAsync(entities);

    public void Update(T entity) => Set.Update(entity);

    public void Remove(T entity) => Set.Remove(entity);

    public Task<int> SaveChangesAsync() => Context.SaveChangesAsync();
}
