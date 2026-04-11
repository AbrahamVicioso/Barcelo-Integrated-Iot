using FluentValidation;
using Usuarios.Application.DTOs.Personal;

namespace Usuarios.Application.Validators.Personal;

public class UpdatePersonalDtoValidator : AbstractValidator<UpdatePersonalDto>
{
    public UpdatePersonalDtoValidator()
    {
        RuleFor(x => x.PersonalId)
            .GreaterThan(0).WithMessage("El PersonalId debe ser mayor a 0");

        RuleFor(x => x.NombreCompleto)
            .NotEmpty().WithMessage("El nombre completo es requerido")
            .MaximumLength(200).WithMessage("El nombre completo no puede exceder 200 caracteres");

        RuleFor(x => x.PuestoId)
            .GreaterThan(0).WithMessage("El PuestoId debe ser mayor a 0");

        RuleFor(x => x.DepartamentoId)
            .GreaterThan(0).WithMessage("El DepartamentoId debe ser mayor a 0");

        RuleFor(x => x.Turno)
            .MaximumLength(20).WithMessage("El turno no puede exceder 20 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Turno));

        RuleFor(x => x.Supervisor)
            .GreaterThan(0).WithMessage("El Supervisor debe ser mayor a 0")
            .When(x => x.Supervisor.HasValue);
    }
}
