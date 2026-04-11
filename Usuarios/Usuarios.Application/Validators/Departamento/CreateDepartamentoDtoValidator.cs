using FluentValidation;
using Usuarios.Application.DTOs.Departamento;

namespace Usuarios.Application.Validators.Departamento;

public class CreateDepartamentoDtoValidator : AbstractValidator<CreateDepartamentoDto>
{
    public CreateDepartamentoDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del departamento es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));
    }
}
