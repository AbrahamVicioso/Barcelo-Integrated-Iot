using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class UpdateReservaActividadCommand : IRequest<Result<ReservaActividadDto>>
{
    public int ReservaActividadId { get; set; }
    public DateTime FechaReserva { get; set; }
    public TimeSpan HoraReserva { get; set; }
    public int NumeroPersonas { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal MontoTotal { get; set; }
    public string? NotasEspeciales { get; set; }
    public bool RecordatorioEnviado { get; set; }
    public DateTime? FechaRecordatorio { get; set; }
}
