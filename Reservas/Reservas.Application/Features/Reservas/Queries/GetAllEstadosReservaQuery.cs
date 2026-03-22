using MediatR;
using Reservas.Application.Common;
using Reservas.Domain.Entites;

namespace Reservas.Application.Features.Reservas.Queries;

public class GetAllEstadosReservaQuery : IRequest<Result<IEnumerable<EstadoReserva>>>
{
}
