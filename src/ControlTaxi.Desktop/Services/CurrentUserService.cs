using ControlTaxi.Application.Common.Interfaces;
using ControlTaxi.Application.Features.Auth;
using ControlTaxi.Domain.Enums;

namespace ControlTaxi.Desktop.Services;

/// <summary>
/// Mantiene la sesión del usuario autenticado en memoria (singleton).
/// La capa de aplicación e infraestructura la consumen vía <see cref="ICurrentUser"/>.
/// </summary>
public sealed class CurrentUserService : ICurrentUser
{
    private HashSet<string> _permisos = new(StringComparer.OrdinalIgnoreCase);

    public bool IsAuthenticated { get; private set; }
    public string? NombreUsuario { get; private set; }
    public RolUsuario Rol { get; private set; }
    public bool EsAdministrador => Rol == RolUsuario.Administrador;
    public IReadOnlyCollection<string> Permisos => _permisos;

    // El Administrador puede ver todo, sin importar la lista de permisos.
    public bool TienePermiso(string permiso) => EsAdministrador || _permisos.Contains(permiso);

    public void SetSession(LoginResult login)
    {
        NombreUsuario = login.NombreUsuario;
        Rol = login.Rol;
        _permisos = new HashSet<string>(login.Permisos, StringComparer.OrdinalIgnoreCase);
        IsAuthenticated = true;
    }

    public void Clear()
    {
        NombreUsuario = null;
        Rol = RolUsuario.Operador;
        _permisos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        IsAuthenticated = false;
    }
}
