using System.Collections.Concurrent;
using ControlTaxi.Application.Common.Interfaces;
using ControlTaxi.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ControlTaxi.Infrastructure.Persistence.Repositories;

/// <summary>
/// Unit of Work sobre EF Core. Cachea repositorios por tipo y expone una
/// transacción explícita para operaciones que abarcan varias entidades.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ControlTaxiDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public UnitOfWork(ControlTaxiDbContext context) => _context = context;

    public IRepository<T> Repository<T>() where T : BaseEntity =>
        (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(_context));

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                await action(ct);
                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }

    public void Dispose() => _context.Dispose();
}
