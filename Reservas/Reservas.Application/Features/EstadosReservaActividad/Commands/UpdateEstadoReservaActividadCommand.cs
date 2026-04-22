using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.EstadosReservaActividad.Commands;

public class UpdateEstadoReservaActividadCommand : IRequest<Result<EstadoReservaActividadDto>>
{
    public int EstadoReservaActividadId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}