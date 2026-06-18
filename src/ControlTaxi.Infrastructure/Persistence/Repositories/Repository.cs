using System.Linq.Expressions;
using ControlTaxi.Application.Common.Interfaces;
using ControlTaxi.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ControlTaxi.Infrastructure.Persistence.Repositories;

/// <summary>Implementación genérica de <see cref="IRepository{T}"/> sobre EF Core.</summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ControlTaxiDbContext _context;
    private readonly DbSet<T> _set;

    public Repository(ControlTaxiDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _set.FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default) =>
        await _set.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await _set.AsNoTracking().Where(predicate).ToListAsync(ct);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await _set.FirstOrDefaultAsync(predicate, ct);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await _set.AnyAsync(predicate, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await _set.AddAsync(entity, ct);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);
}
