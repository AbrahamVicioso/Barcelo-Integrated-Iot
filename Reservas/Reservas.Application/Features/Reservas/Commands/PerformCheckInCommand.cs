using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Reservas.Commands;

public class PerformCheckInCommand : IRequest<Result<CheckInDto>>
{
    public int ReservaId { get; set; }
}
