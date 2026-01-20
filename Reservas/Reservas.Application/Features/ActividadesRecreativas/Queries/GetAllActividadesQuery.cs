using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.ActividadesRecreativas.Queries;

public class GetAllActividadesQuery : IRequest<Result<IEnumerable<ActividadRecreativaDto>>>
{
}
