using MediatR;
using Usuarios.Application.DTOs.Dashboard;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Dashboard.Queries.GetHuespedesStats;

public class GetHuespedesStatsQueryHandler : IRequestHandler<GetHuespedesStatsQuery, HuespedesStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetHuespedesStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<HuespedesStatsDto> Handle(GetHuespedesStatsQuery request, CancellationToken cancellationToken)
    {
        var huespedes = await _unitOfWork.Huespedes.GetAllAsync();
        var huespedesList = huespedes.ToList();

        // TODO: Filtrar por hotel si se especifica (requiere relación Huespede-Hotel)
        // Por ahora devolvemos todos los huéspedes

        var stats = new HuespedesStatsDto
        {
            Total = huespedesList.Count,
            Vip = huespedesList.Count(h => h.EsVip),
            PorTipoDocumento = huespedesList
                .GroupBy(h => h.TipoDocumentoNavigation?.Nombre ?? "Sin especificar")
                .Select(g => new HuespedesPorTipoDocumentoDto
                {
                    Tipo = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList()
        };

        return stats;
    }
}
