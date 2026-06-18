using ControlTaxi.Domain.Authorization;
using ControlTaxi.Domain.SharedKernel;

namespace ControlTaxi.Application.Features.Usuarios;

/// <summary>Casos de uso del módulo Usuarios, roles y permisos.</summary>
public interface IUsuariosService
{
    Task<Result<IReadOnlyList<UsuarioRowDto>>> ListarAsync(CancellationToken ct = default);
    Task<Result<UsuarioRowDto>> ObtenerAsync(string nombreUsuario, CancellationToken ct = default);

    /// <summary>Crea o actualiza un usuario (upsert por nombre de usuario). Devuelve su Id.</summary>
    Task<Result<int>> GuardarAsync(GuardarUsuarioRequest request, string usuarioActor, CancellationToken ct = default);

    /// <summary>Catálogo de módulos sobre los que se pueden conceder permisos.</summary>
    IReadOnlyList<ModuloPermiso> CatalogoPermisos();
}
