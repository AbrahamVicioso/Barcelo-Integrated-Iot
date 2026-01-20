using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Reservas.Commands;

public class UpdateReservaCommand : IRequest<Result<ReservaDto>>
{
    public int ReservaId { get; set; }
    public int HabitacionId { get; set; }
    public DateTime FechaCheckIn { get; set; }
    public DateTime FechaCheckOut { get; set; }
    public int NumeroHuespedes { get; set; }
    public int NumeroNinos { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal MontoPagado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime? CheckInRealizado { get; set; }
    public DateTime? CheckOutRealizado { get; set; }
    public string? Observaciones { get; set; }
}
