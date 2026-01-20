using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class CreateReservaActividadCommand : IRequest<Result<ReservaActividadDto>>
{
    public int ActividadId { get; set; }
    public int HuespedId { get; set; }
    public DateTime FechaReserva { get; set; }
    public TimeSpan HoraReserva { get; set; }
    public int NumeroPersonas { get; set; }
    public decimal MontoTotal { get; set; }
    public string? NotasEspeciales { get; set; }
}
