using FluentValidation;
using Usuarios.Application.DTOs.Huespedes;

namespace Usuarios.Application.Validators.Huespedes;

public class UpdateHuespedeDtoValidator : AbstractValidator<UpdateHuespedeDto>
{
    public UpdateHuespedeDtoValidator()
    {
        RuleFor(x => x.HuespedId)
            .GreaterThan(0).WithMessage("El HuespedId debe ser mayor a 0");

        RuleFor(x => x.NombreCompleto)
            .NotEmpty().WithMessage("El nombre completo es requerido")
            .MaximumLength(200).WithMessage("El nombre completo no puede exceder 200 caracteres");

        RuleFor(x => x.ContactoEmergencia)
            .MaximumLength(200).WithMessage("El contacto de emergencia no puede exceder 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.ContactoEmergencia));

        RuleFor(x => x.TelefonoEmergencia)
            .MaximumLength(50).WithMessage("El teléfono de emergencia no puede exceder 50 caracteres")
            .When(x => !string.IsNullOrEmpty(x.TelefonoEmergencia));

        RuleFor(x => x.PreferenciasAlimentarias)
            .MaximumLength(500).WithMessage("Las preferencias alimentarias no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.PreferenciasAlimentarias));

        RuleFor(x => x.NotasEspeciales)
            .MaximumLength(1000).WithMessage("Las notas especiales no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.NotasEspeciales));
    }
}
