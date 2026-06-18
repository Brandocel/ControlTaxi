using FluentValidation;

namespace ControlTaxi.Application.Features.Relaciones;

public sealed class GuardarRelacionValidator : AbstractValidator<GuardarRelacionRequest>
{
    public GuardarRelacionValidator()
    {
        RuleFor(x => x.FolioApp)
            .NotEmpty().WithMessage("El folio del ticket (app) es obligatorio.")
            .MaximumLength(60);

        RuleFor(x => x.FolioOperacion).MaximumLength(60);
        RuleFor(x => x.FolioPos).MaximumLength(120);
        RuleFor(x => x.Gafete).MaximumLength(300);
        RuleFor(x => x.TaxistaNombre).MaximumLength(150);
        RuleFor(x => x.TransporteTipo).MaximumLength(20);
        RuleFor(x => x.Observaciones).MaximumLength(300);

        // Para guardar debe haber algo útil: un taxista o un ticket POS.
        RuleFor(x => x)
            .Must(x => x.TaxistaId > 0 || !string.IsNullOrWhiteSpace(x.FolioOperacion))
            .WithMessage("Debe ligar un taxista o asignar un ticket POS.");
    }
}
