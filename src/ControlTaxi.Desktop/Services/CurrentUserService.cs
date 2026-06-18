using ControlTaxi.Application.Common.Interfaces;
using ControlTaxi.Application.Features.Auth;

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
    public IReadOnlyCollection<string> Permisos => _permisos;

    public bool TienePermiso(string permiso) => _permisos.Contains(permiso);

    public void SetSession(LoginResult login)
    {
        NombreUsuario = login.NombreUsuario;
        _permisos = new HashSet<string>(login.Permisos, StringComparer.OrdinalIgnoreCase);
        IsAuthenticated = true;
    }

    public void Clear()
    {
        NombreUsuario = null;
        _permisos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        IsAuthenticated = false;
    }
}
