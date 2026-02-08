using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.Dispositivos.Commands;

public class CreateDispositivoCommand : IRequest<Result<int>>
{
    public CreateDispositivoDto Dispositivo { get; set; } = new();
}
