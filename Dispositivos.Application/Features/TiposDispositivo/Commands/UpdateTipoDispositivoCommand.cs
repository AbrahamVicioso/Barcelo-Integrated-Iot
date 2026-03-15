using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.TiposDispositivo.Commands;

public class UpdateTipoDispositivoCommand : IRequest<Result<TipoDispositivoDto>>
{
    public int TipoDispositivoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
