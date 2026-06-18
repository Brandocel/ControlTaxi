using ControlTaxi.Application.Common.Interfaces;
using ControlTaxi.Domain.Entities;
using ControlTaxi.Domain.SharedKernel;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ControlTaxi.Application.Features.Auth;

/// <summary>
/// Autentica usuarios. Devuelve siempre un <see cref="Result"/> explícito:
/// nunca lanza ni devuelve null en flujos esperados, y registra cada intento.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IValidator<LoginRequest> _validator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IValidator<LoginRequest> validator,
        ILogger<AuthService> logger)
    {
        _uow = uow;
        _hasher = hasher;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<LoginResult>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var message = string.Join(" ", validation.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<LoginResult>(Error.Validation(message));
        }

        var usuario = await _uow.Repository<Usuario>()
            .FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario, ct);

        if (usuario is null || !_hasher.Verify(request.Password, usuario.PasswordHash))
        {
            // Mismo mensaje para usuario inexistente y contraseña incorrecta (no filtrar info).
            _logger.LogWarning("Intento de login fallido para el usuario {Usuario}.", request.NombreUsuario);
            return Result.Failure<LoginResult>(Error.Unauthorized("Usuario o contraseña incorrectos."));
        }

        if (!usuario.EstaActivo)
        {
            _logger.LogWarning("Login bloqueado: el usuario {Usuario} está inactivo.", usuario.NombreUsuario);
            return Result.Failure<LoginResult>(Error.Unauthorized("El usuario está inactivo."));
        }

        _logger.LogInformation("Login exitoso para {Usuario} (rol {Rol}).", usuario.NombreUsuario, usuario.Rol);
        return Result.Success(new LoginResult(
            usuario.NombreUsuario,
            usuario.Nombre,
            usuario.Rol,
            usuario.Permisos.ToList()));
    }
}
