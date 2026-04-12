using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Reservas.Queries;

public class GetAllReservasQuery : IRequest<Result<IEnumerable<ReservaDto>>>
{
    public int? EstadoReservaId { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? NombreHuesped { get; set; }
}
