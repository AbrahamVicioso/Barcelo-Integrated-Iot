using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Reservas.Commands;

public class PerformCheckInCommand : IRequest<Result<CheckInDto>>
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string NumeroReserva { get; set; } = string.Empty;
}
