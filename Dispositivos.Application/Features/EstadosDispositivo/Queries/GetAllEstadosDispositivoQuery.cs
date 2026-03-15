using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.EstadosDispositivo.Queries;

public class GetAllEstadosDispositivoQuery : IRequest<Result<IEnumerable<EstadoDispositivoDto>>>
{
}
