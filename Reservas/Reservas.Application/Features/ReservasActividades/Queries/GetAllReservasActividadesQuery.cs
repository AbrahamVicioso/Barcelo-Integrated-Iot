using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.ReservasActividades.Queries;

public class GetAllReservasActividadesQuery : IRequest<Result<IEnumerable<ReservaActividadDto>>>
{
}
