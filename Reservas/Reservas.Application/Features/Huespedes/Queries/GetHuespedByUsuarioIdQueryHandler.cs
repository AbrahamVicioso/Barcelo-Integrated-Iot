using MediatR;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Huespedes.Queries;

public class GetHuespedByUsuarioIdQueryHandler : IRequestHandler<GetHuespedByUsuarioIdQuery, HuespedeDto?>
{
    private readonly IUsuariosApiService _usuariosApiService;

    public GetHuespedByUsuarioIdQueryHandler(IUsuariosApiService usuariosApiService)
    {
        _usuariosApiService = usuariosApiService;
    }

    public async Task<HuespedeDto?> Handle(GetHuespedByUsuarioIdQuery request, CancellationToken cancellationToken)
    {
        return await _usuariosApiService.GetHuespedByUsuarioIdAsync(request.UsuarioId, cancellationToken);
    }
}
