using FluentValidation;

namespace ControlTaxi.Application.Features.Auth;

public sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.NombreUsuario)
            .NotEmpty().WithMessage("El usuario es obligatorio.")
            .MaximumLength(50);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}
