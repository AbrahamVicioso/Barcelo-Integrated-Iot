using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Reservas.Commands;

public class PerformCheckOutCommand : IRequest<Result<ReservaDto>>
{
    public int ReservaId { get; set; }
}
