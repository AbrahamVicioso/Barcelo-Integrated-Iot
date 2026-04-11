using FluentValidation;
using Usuarios.Application.DTOs.Puesto;

namespace Usuarios.Application.Validators.Puesto;

public class CreatePuestoDtoValidator : AbstractValidator<CreatePuestoDto>
{
    public CreatePuestoDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del puesto es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));
    }
}
