using ControlTaxi.Domain.Enums;

namespace ControlTaxi.Application.Features.Usuarios;

/// <summary>Fila de usuario para la lista.</summary>
public sealed class UsuarioRowDto
{
    public int Id { get; init; }
    public string NombreUsuario { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public RolUsuario Rol { get; init; }
    public string RolTexto { get; init; } = string.Empty;
    public bool Activo { get; init; }
    public string EstatusTexto { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Permisos { get; init; } = Array.Empty<string>();
    public string CreadoEn { get; init; } = string.Empty;
}

/// <summary>
/// Datos para crear o actualizar un usuario. En edición, <see cref="Password"/>
/// puede ir vacío para conservar la contraseña actual.
/// </summary>
public sealed record GuardarUsuarioRequest(
    string NombreUsuario,
    string Nombre,
    string? Password,
    RolUsuario Rol,
    bool Activo,
    IReadOnlyCollection<string> Permisos);
