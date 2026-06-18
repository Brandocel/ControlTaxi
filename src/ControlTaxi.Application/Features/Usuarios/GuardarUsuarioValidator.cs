using FluentValidation;

namespace ControlTaxi.Application.Features.Usuarios;

public sealed class GuardarUsuarioValidator : AbstractValidator<GuardarUsuarioRequest>
{
    public GuardarUsuarioValidator()
    {
        RuleFor(x => x.NombreUsuario)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MaximumLength(50)
            .Matches("^[A-Za-z0-9._-]+$").WithMessage("El usuario solo admite letras, números, punto, guión y guión bajo.");

        RuleFor(x => x.Nombre).MaximumLength(150);

        // La contraseña, cuando se proporciona, debe tener al menos 6 caracteres.
        RuleFor(x => x.Password!)
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Password));
    }
}
