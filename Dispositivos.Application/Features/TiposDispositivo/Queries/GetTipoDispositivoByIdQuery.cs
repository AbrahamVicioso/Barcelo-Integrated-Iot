using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.TiposDispositivo.Queries;

public class GetTipoDispositivoByIdQuery : IRequest<Result<TipoDispositivoDto>>
{
    public int TipoDispositivoId { get; set; }
}
