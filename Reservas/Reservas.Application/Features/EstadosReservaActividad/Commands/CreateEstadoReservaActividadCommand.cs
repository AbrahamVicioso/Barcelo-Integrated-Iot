using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.EstadosReservaActividad.Commands;

public class CreateEstadoReservaActividadCommand : IRequest<Result<EstadoReservaActividadDto>>
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}