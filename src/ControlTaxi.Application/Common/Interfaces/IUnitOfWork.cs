using ControlTaxi.Domain.Common;

namespace ControlTaxi.Application.Common.Interfaces;

/// <summary>
/// Unit of Work: agrupa el acceso a repositorios y confirma todos los cambios
/// en una sola transacción. Garantiza consistencia (cobro + comisión + corte
/// se guardan juntos o no se guardan).
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default);
}
