using MediatR;
using Reservas.Application.Common;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class DeleteReservaActividadCommand : IRequest<Result<bool>>
{
    public int ReservaActividadId { get; set; }
}
