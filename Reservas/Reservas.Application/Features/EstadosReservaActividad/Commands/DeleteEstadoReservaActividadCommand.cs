using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.EstadosReservaActividad.Commands;

public class DeleteEstadoReservaActividadCommand : IRequest<Result<bool>>
{
    public int EstadoReservaActividadId { get; set; }
}