using ControlTaxi.Application.Common.Interfaces;
using ControlTaxi.Domain.Authorization;
using ControlTaxi.Domain.Entities;
using ControlTaxi.Domain.Enums;
using ControlTaxi.Domain.SharedKernel;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ControlTaxi.Application.Features.Usuarios;

/// <summary>
/// Gestiona usuarios, roles y permisos. La contraseña solo se actualiza cuando
/// se proporciona; nunca se expone el hash. Devuelve <see cref="Result"/> y registra.
/// </summary>
public sealed class UsuariosService : IUsuariosService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IValidator<GuardarUsuarioRequest> _validator;
    private readonly ILogger<UsuariosService> _logger;

    public UsuariosService(
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IValidator<GuardarUsuarioRequest> validator,
        ILogger<UsuariosService> logger)
    {
        _uow = uow;
        _hasher = hasher;
        _validator = validator;
        _logger = logger;
    }

    public IReadOnlyList<ModuloPermiso> CatalogoPermisos() => Permisos.Catalogo;

    public async Task<Result<IReadOnlyList<UsuarioRowDto>>> ListarAsync(CancellationToken ct = default)
    {
        var usuarios = await _uow.Repository<Usuario>().ListAsync(ct);
        var filas = usuarios.OrderBy(u => u.NombreUsuario).Select(Map).ToList();
        return Result.Success<IReadOnlyList<UsuarioRowDto>>(filas);
    }

    public async Task<Result<UsuarioRowDto>> ObtenerAsync(string nombreUsuario, CancellationToken ct = default)
    {
        var nombre = (nombreUsuario ?? string.Empty).Trim();
        var usuario = await _uow.Repository<Usuario>().FirstOrDefaultAsync(u => u.NombreUsuario == nombre, ct);
        return usuario is null
            ? Result.Failure<UsuarioRowDto>(Error.NotFound("No se encontró el usuario."))
            : Result.Success(Map(usuario));
    }

    public async Task<Result<int>> GuardarAsync(GuardarUsuarioRequest request, string usuarioActor, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure<int>(Error.Validation(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage))));

        var nombreUsuario = request.NombreUsuario.Trim();
        var repo = _uow.Repository<Usuario>();
        var usuario = await repo.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario, ct);
        var esNuevo = usuario is null;

        var permisosValidos = Permisos.SoloValidos(request.Permisos ?? Array.Empty<string>()).ToList();

        if (esNuevo)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                return Result.Failure<int>(Error.Validation("La contraseña es obligatoria para un usuario nuevo."));

            usuario = new Usuario(nombreUsuario, request.Nombre, _hasher.Hash(request.Password.Trim()), request.Rol);
        }
        else
        {
            usuario!.ActualizarNombre(request.Nombre);
            usuario.CambiarRol(request.Rol);
            if (!string.IsNullOrWhiteSpace(request.Password))
                usuario.CambiarPassword(_hasher.Hash(request.Password.Trim()));
        }

        if (request.Activo) usuario!.Activar(); else usuario!.Desactivar();
        usuario.EstablecerPermisos(permisosValidos);

        if (esNuevo)
            await repo.AddAsync(usuario, ct);
        else
            repo.Update(usuario);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Usuario {Estado} {Usuario} (rol {Rol}, {NumPermisos} permisos) por {Actor}.",
            esNuevo ? "creado" : "actualizado", nombreUsuario, request.Rol, permisosValidos.Count, usuarioActor);

        return Result.Success(usuario.Id);
    }

    private static UsuarioRowDto Map(Usuario u) => new()
    {
        Id = u.Id,
        NombreUsuario = u.NombreUsuario,
        Nombre = u.Nombre,
        Rol = u.Rol,
        RolTexto = TextoRol(u.Rol),
        Activo = u.EstaActivo,
        EstatusTexto = u.EstaActivo ? "Activo" : "Inactivo",
        Permisos = u.Permisos.ToList(),
        CreadoEn = u.CreadoEn == default ? string.Empty : u.CreadoEn.ToString("dd/MM/yyyy")
    };

    private static string TextoRol(RolUsuario rol) => rol switch
    {
        RolUsuario.Operador => "Operador",
        RolUsuario.Supervisor => "Supervisor",
        RolUsuario.Administrador => "Administrador",
        _ => rol.ToString()
    };
}
