namespace ControlTaxi.Application.Common.Interfaces;

/// <summary>
/// Sesión del usuario autenticado. La UI la rellena tras el login y la
/// infraestructura la usa para auditoría (CreadoPor/ModificadoPor).
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? NombreUsuario { get; }
    IReadOnlyCollection<string> Permisos { get; }
    bool TienePermiso(string permiso);
}
