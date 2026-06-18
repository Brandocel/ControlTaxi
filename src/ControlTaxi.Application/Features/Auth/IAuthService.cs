using ControlTaxi.Domain.SharedKernel;

namespace ControlTaxi.Application.Features.Auth;

/// <summary>Servicio de aplicación para autenticación.</summary>
public interface IAuthService
{
    Task<Result<LoginResult>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
