using MediatR;
using Usuarios.Application.DTOs.Dashboard;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Dashboard.Queries.GetPersonalStats;

public class GetPersonalStatsQueryHandler : IRequestHandler<GetPersonalStatsQuery, PersonalStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPersonalStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PersonalStatsDto> Handle(GetPersonalStatsQuery request, CancellationToken cancellationToken)
    {
        var personal = await _unitOfWork.Personal.GetAllAsync();

        // Filtrar por hotel si se especifica
        if (request.HotelId.HasValue)
        {
            personal = personal.Where(p => p.HotelId == request.HotelId.Value);
        }

        var personalList = personal.ToList();

        var stats = new PersonalStatsDto
        {
            Total = personalList.Count,
            Activos = personalList.Count(p => p.EstaActivo),
            Inactivos = personalList.Count(p => !p.EstaActivo),
            PorDepartamento = personalList
                .GroupBy(p => p.Departamento ?? "Sin departamento")
                .Select(g => new PersonalPorDepartamentoDto
                {
                    Departamento = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList()
        };

        return stats;
    }
}
