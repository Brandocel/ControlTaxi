using ControlTaxi.Domain.Common;
using ControlTaxi.Domain.Enums;

namespace ControlTaxi.Domain.Entities;

/// <summary>
/// Catálogo de taxistas. Es el "quién" al que se liga un ticket en el módulo
/// de Relaciones. La clave es el identificador de negocio (legacy: id_catalogo).
/// </summary>
public class Taxista : AuditableEntity
{
    public string Clave { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string Telefono { get; private set; } = string.Empty;
    public string Unidad { get; private set; } = string.Empty;
    public string Placas { get; private set; } = string.Empty;
    public EstatusGeneral Estatus { get; private set; }

    private Taxista() { }

    public Taxista(string clave, string nombre, string? telefono = null, string? unidad = null, string? placas = null)
    {
        if (string.IsNullOrWhiteSpace(clave))
            throw new ArgumentException("La clave del taxista es obligatoria.", nameof(clave));
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del taxista es obligatorio.", nameof(nombre));

        Clave = clave.Trim();
        Nombre = nombre.Trim();
        Telefono = (telefono ?? string.Empty).Trim();
        Unidad = (unidad ?? string.Empty).Trim();
        Placas = (placas ?? string.Empty).Trim();
        Estatus = EstatusGeneral.Activo;
    }

    public bool EstaActivo => Estatus == EstatusGeneral.Activo;

    public void Actualizar(string nombre, string? telefono, string? unidad, string? placas)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del taxista es obligatorio.", nameof(nombre));
        Nombre = nombre.Trim();
        Telefono = (telefono ?? string.Empty).Trim();
        Unidad = (unidad ?? string.Empty).Trim();
        Placas = (placas ?? string.Empty).Trim();
    }

    public void Activar() => Estatus = EstatusGeneral.Activo;
    public void Desactivar() => Estatus = EstatusGeneral.Inactivo;
}
