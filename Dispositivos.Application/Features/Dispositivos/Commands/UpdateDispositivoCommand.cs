using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.Dispositivos.Commands;

public class UpdateDispositivoCommand : IRequest<Result<DispositivoDto>>
{
    public int DispositivoId { get; set; }
    public UpdateDispositivoDto Dispositivo { get; set; } = new();
}
