using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.EstadosDispositivo.Commands;

public class UpdateEstadoDispositivoCommand : IRequest<Result<EstadoDispositivoDto>>
{
    public int EstadoDispositivoId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
