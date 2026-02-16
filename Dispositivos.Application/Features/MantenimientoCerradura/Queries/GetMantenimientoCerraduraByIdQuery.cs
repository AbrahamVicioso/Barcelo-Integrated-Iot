using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.MantenimientoCerradura.Queries;

public class GetMantenimientoCerraduraByIdQuery : IRequest<Result<MantenimientoCerraduraDto>>
{
    public int MantenimientoId { get; set; }
}
