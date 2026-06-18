using System.Linq.Expressions;
using ControlTaxi.Domain.Common;

namespace ControlTaxi.Application.Common.Interfaces;

/// <summary>
/// Repositorio genérico de solo-lectura/escritura sobre una entidad agregada.
/// Reemplaza el SQL crudo y los Dictionary&lt;string,object&gt; del sistema anterior.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}
