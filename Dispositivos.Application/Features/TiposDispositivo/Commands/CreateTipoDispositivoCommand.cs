using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.TiposDispositivo.Commands;

public class CreateTipoDispositivoCommand : IRequest<Result<int>>
{
    public CreateTipoDispositivoDto TipoDispositivo { get; set; } = new();
}
