using FluentValidation;
using Usuarios.Application.DTOs.Personal;

namespace Usuarios.Application.Validators.Personal;

public class CreatePersonalDtoValidator : AbstractValidator<CreatePersonalDto>
{
    public CreatePersonalDtoValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("El UsuarioId es requerido");

        RuleFor(x => x.HotelId)
            .GreaterThan(0).WithMessage("El HotelId debe ser mayor a 0");

        RuleFor(x => x.NombreCompleto)
            .NotEmpty().WithMessage("El nombre completo es requerido")
            .MaximumLength(200).WithMessage("El nombre completo no puede exceder 200 caracteres");

        RuleFor(x => x.Puesto)
            .NotEmpty().WithMessage("El puesto es requerido")
            .MaximumLength(100).WithMessage("El puesto no puede exceder 100 caracteres");

        RuleFor(x => x.Departamento)
            .NotEmpty().WithMessage("El departamento es requerido")
            .MaximumLength(100).WithMessage("El departamento no puede exceder 100 caracteres");

        RuleFor(x => x.NumeroEmpleado)
            .NotEmpty().WithMessage("El número de empleado es requerido")
            .MaximumLength(50).WithMessage("El número de empleado no puede exceder 50 caracteres");

        RuleFor(x => x.FechaContratacion)
            .NotEmpty().WithMessage("La fecha de contratación es requerida")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("La fecha de contratación no puede ser futura");

        RuleFor(x => x.Turno)
            .MaximumLength(20).WithMessage("El turno no puede exceder 20 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Turno));

        RuleFor(x => x.Supervisor)
            .GreaterThan(0).WithMessage("El Supervisor debe ser mayor a 0")
            .When(x => x.Supervisor.HasValue);
    }
}
