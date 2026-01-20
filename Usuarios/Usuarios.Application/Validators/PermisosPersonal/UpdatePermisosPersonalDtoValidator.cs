using FluentValidation;
using Usuarios.Application.DTOs.PermisosPersonal;

namespace Usuarios.Application.Validators.PermisosPersonal;

public class UpdatePermisosPersonalDtoValidator : AbstractValidator<UpdatePermisosPersonalDto>
{
    public UpdatePermisosPersonalDtoValidator()
    {
        RuleFor(x => x.PermisoId)
            .GreaterThan(0).WithMessage("El PermisoId debe ser mayor a 0");

        RuleFor(x => x.FechaExpiracion)
            .GreaterThan(DateTime.UtcNow).WithMessage("La fecha de expiración debe ser futura")
            .When(x => x.FechaExpiracion.HasValue);

        RuleFor(x => x.Justificacion)
            .MaximumLength(500).WithMessage("La justificación no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Justificacion));
    }
}
