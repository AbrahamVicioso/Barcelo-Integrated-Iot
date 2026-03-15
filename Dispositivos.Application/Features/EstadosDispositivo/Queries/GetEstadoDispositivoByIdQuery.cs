using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.EstadosDispositivo.Queries;

public class GetEstadoDispositivoByIdQuery : IRequest<Result<EstadoDispositivoDto>>
{
    public int EstadoDispositivoId { get; set; }
}
