using ControlTaxi.Application.Features.Auth;
using ControlTaxi.Application.Features.Relaciones;
using ControlTaxi.Application.Features.Usuarios;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ControlTaxi.Application;

/// <summary>Registra los servicios de la capa de aplicación en el contenedor DI.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginValidator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRelacionesService, RelacionesService>();
        services.AddScoped<IUsuariosService, UsuariosService>();
        return services;
    }
}
