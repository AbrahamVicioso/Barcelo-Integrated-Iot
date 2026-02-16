using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.MantenimientoCerradura.Commands;

public class CreateMantenimientoCerraduraCommand : IRequest<Result<int>>
{
    public CreateMantenimientoCerraduraDto Mantenimiento { get; set; } = new();
}
