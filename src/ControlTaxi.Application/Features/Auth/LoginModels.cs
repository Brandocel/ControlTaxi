using ControlTaxi.Domain.Enums;

namespace ControlTaxi.Application.Features.Auth;

/// <summary>Datos de entrada para autenticar a un usuario.</summary>
public sealed record LoginRequest(string NombreUsuario, string Password);

/// <summary>Datos de la sesión devueltos tras un login exitoso.</summary>
public sealed record LoginResult(
    string NombreUsuario,
    string Nombre,
    RolUsuario Rol,
    IReadOnlyCollection<string> Permisos);
