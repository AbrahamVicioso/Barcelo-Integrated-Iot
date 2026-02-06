using MediatR;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Huespedes.Queries;

public class GetHuespedByUsuarioIdQuery : IRequest<HuespedeDto?>
{
    public string UsuarioId { get; set; } = string.Empty;

    public GetHuespedByUsuarioIdQuery(string usuarioId)
    {
        UsuarioId = usuarioId;
    }
}
