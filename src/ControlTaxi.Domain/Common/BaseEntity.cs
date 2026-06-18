namespace ControlTaxi.Domain.Common;

/// <summary>Entidad base con identidad. Todas las entidades del dominio heredan de aquí.</summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}

/// <summary>
/// Contrato de auditoría. La infraestructura rellena estos campos automáticamente
/// en SaveChanges, de modo que el dominio no se preocupa por el "quién/cuándo".
/// </summary>
public interface IAuditable
{
    DateTime CreadoEn { get; set; }
    string? CreadoPor { get; set; }
    DateTime? ModificadoEn { get; set; }
    string? ModificadoPor { get; set; }
}

/// <summary>Entidad base auditable (created/modified). Base para la mayoría de entidades de negocio.</summary>
public abstract class AuditableEntity : BaseEntity, IAuditable
{
    public DateTime CreadoEn { get; set; }
    public string? CreadoPor { get; set; }
    public DateTime? ModificadoEn { get; set; }
    public string? ModificadoPor { get; set; }
}
