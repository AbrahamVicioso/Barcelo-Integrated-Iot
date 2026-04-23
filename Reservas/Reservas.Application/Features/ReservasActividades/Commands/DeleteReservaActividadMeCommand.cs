using MediatR;
using Reservas.Application.Common;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class DeleteReservaActividadMeCommand : IRequest<Result<bool>>
{
    public string UsuarioId { get; set; } = string.Empty;
    public int ReservaActividadId { get; set; }
}