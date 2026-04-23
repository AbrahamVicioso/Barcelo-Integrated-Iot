using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.ReservasActividades.Queries;

public class GetReservasActividadByUsuarioIdQuery : IRequest<Result<IEnumerable<ReservaActividadDto>>>
{
    public string UsuarioId { get; set; } = string.Empty;

    public GetReservasActividadByUsuarioIdQuery(string usuarioId)
    {
        UsuarioId = usuarioId;
    }
}