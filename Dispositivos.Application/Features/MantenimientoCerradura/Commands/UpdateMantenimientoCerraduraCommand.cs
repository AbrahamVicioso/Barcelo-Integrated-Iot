using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.MantenimientoCerradura.Commands;

public class UpdateMantenimientoCerraduraCommand : IRequest<Result<MantenimientoCerraduraDto>>
{
    public UpdateMantenimientoCerraduraDto Mantenimiento { get; set; } = new();
}
