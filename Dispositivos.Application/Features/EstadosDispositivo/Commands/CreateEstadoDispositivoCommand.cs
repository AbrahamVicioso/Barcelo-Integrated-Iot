using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.EstadosDispositivo.Commands;

public class CreateEstadoDispositivoCommand : IRequest<Result<int>>
{
    public CreateEstadoDispositivoDto EstadoDispositivo { get; set; } = new();
}
