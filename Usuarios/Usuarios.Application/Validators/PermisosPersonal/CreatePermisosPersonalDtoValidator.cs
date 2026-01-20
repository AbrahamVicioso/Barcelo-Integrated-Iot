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

        RuleFor(x => x.TipoPermiso)
            .NotEmpty().WithMessage("El tipo de permiso es requerido")
            .MaximumLength(50).WithMessage("El tipo de permiso no puede exceder 50 caracteres");

        RuleFor(x => x.OtorgadoPor)
            .NotEmpty().WithMessage("El campo OtorgadoPor es requerido")
            .MaximumLength(450).WithMessage("El campo OtorgadoPor no puede exceder 450 caracteres");

        RuleFor(x => x.FechaExpiracion)
            .GreaterThan(DateTime.UtcNow).WithMessage("La fecha de expiración debe ser futura")
            .When(x => x.FechaExpiracion.HasValue);

        RuleFor(x => x.FechaExpiracion)
            .NotNull().WithMessage("Los permisos temporales requieren fecha de expiración")
            .When(x => x.EsTemporal);

        RuleFor(x => x.Justificacion)
            .MaximumLength(500).WithMessage("La justificación no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Justificacion));
    }
}
