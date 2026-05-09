using FluentValidation;
using Usuarios.Application.DTOs.PermisosPersonal;

namespace Usuarios.Application.Validators.PermisosPersonal;

public class CreatePermisosPersonalDtoValidator : AbstractValidator<CreatePermisosPersonalDto>
{
    public CreatePermisosPersonalDtoValidator()
    {
        RuleFor(x => x.PersonalId)
            .GreaterThan(0).WithMessage("El PersonalId debe ser mayor a 0");

        RuleFor(x => x.HabitacionId)
            .GreaterThan(0).WithMessage("El HabitacionId debe ser mayor a 0")
            .When(x => x.HabitacionId.HasValue);

        RuleFor(x => x.ActividadId)
            .GreaterThan(0).WithMessage("El ActividadId debe ser mayor a 0")
            .When(x => x.ActividadId.HasValue);

        RuleFor(x => x)
            .Must(x => x.HabitacionId.HasValue || x.ActividadId.HasValue)
            .WithMessage("Debe especificar al menos una HabitacionId o ActividadId");

        RuleFor(x => x.FechaExpiracion)
            .GreaterThan(DateTime.Now).WithMessage("La fecha de expiración debe ser futura")
            .When(x => x.FechaExpiracion.HasValue);

        RuleFor(x => x.FechaExpiracion)
            .NotNull().WithMessage("Los permisos temporales requieren fecha de expiración")
            .When(x => x.EsTemporal);

        RuleFor(x => x.Justificacion)
            .MaximumLength(500).WithMessage("La justificación no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Justificacion));
    }
}
