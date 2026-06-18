using FluentValidation;

namespace ControlTaxi.Application.Features.Taxistas;

public sealed class GuardarTaxistaValidator : AbstractValidator<GuardarTaxistaRequest>
{
    public GuardarTaxistaValidator()
    {
        RuleFor(x => x.Clave)
            .NotEmpty().WithMessage("La clave es obligatoria.")
            .MaximumLength(20);

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(150);

        RuleFor(x => x.Telefono).MaximumLength(30);
        RuleFor(x => x.Unidad).MaximumLength(30);
        RuleFor(x => x.Placas).MaximumLength(20);
    }
}
