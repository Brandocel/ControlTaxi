using ControlTaxi.Domain.Common;
using ControlTaxi.Domain.Enums;

namespace ControlTaxi.Domain.Entities;

/// <summary>
/// Usuario del POS. La contraseña NUNCA se guarda en claro: solo el hash + salt.
/// Las reglas de cambio de estado/contraseña viven aquí (lógica de dominio rica),
/// no dispersas en servicios.
/// </summary>
public class Usuario : AuditableEntity
{
    public string NombreUsuario { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public RolUsuario Rol { get; private set; }
    public EstatusGeneral Estatus { get; private set; }

    private readonly List<string> _permisos = new();
    public IReadOnlyCollection<string> Permisos => _permisos.AsReadOnly();

    // EF Core necesita un constructor sin parámetros.
    private Usuario() { }

    public Usuario(string nombreUsuario, string nombre, string passwordHash, RolUsuario rol)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("El hash de contraseña es obligatorio.", nameof(passwordHash));

        NombreUsuario = nombreUsuario.Trim();
        Nombre = (nombre ?? string.Empty).Trim();
        PasswordHash = passwordHash;
        Rol = rol;
        Estatus = EstatusGeneral.Activo;
    }

    public bool EstaActivo => Estatus == EstatusGeneral.Activo;

    public void ActualizarNombre(string nombre) => Nombre = (nombre ?? string.Empty).Trim();

    public void CambiarPassword(string nuevoHash)
    {
        if (string.IsNullOrWhiteSpace(nuevoHash))
            throw new ArgumentException("El hash de contraseña es obligatorio.", nameof(nuevoHash));
        PasswordHash = nuevoHash;
    }

    public void Activar() => Estatus = EstatusGeneral.Activo;
    public void Desactivar() => Estatus = EstatusGeneral.Inactivo;
    public void CambiarRol(RolUsuario rol) => Rol = rol;

    public void EstablecerPermisos(IEnumerable<string> permisos)
    {
        _permisos.Clear();
        _permisos.AddRange(permisos
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase));
    }

    public bool TienePermiso(string permiso) =>
        Rol == RolUsuario.Administrador ||
        _permisos.Contains(permiso, StringComparer.OrdinalIgnoreCase);
}
