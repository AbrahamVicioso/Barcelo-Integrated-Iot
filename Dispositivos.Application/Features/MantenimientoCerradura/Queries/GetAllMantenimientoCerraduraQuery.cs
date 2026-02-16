using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.MantenimientoCerradura.Queries;

public class GetAllMantenimientoCerraduraQuery : IRequest<Result<IEnumerable<MantenimientoCerraduraDto>>>
{
}
