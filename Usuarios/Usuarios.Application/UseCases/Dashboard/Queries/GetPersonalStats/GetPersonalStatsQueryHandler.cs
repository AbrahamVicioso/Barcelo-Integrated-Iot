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

        var departamentos = await _unitOfWork.Departamentos.GetAllAsync();
        var nombreDepartamento = departamentos.ToDictionary(d => d.DepartamentoId, d => d.Nombre);

        var stats = new PersonalStatsDto
        {
            Total = personalList.Count,
            Activos = personalList.Count(p => p.EstaActivo),
            Inactivos = personalList.Count(p => !p.EstaActivo),
            PorDepartamento = personalList
                .GroupBy(p => p.DepartamentoId)
                .Select(g => new PersonalPorDepartamentoDto
                {
                    Departamento = nombreDepartamento.TryGetValue(g.Key, out var nombre) ? nombre : g.Key.ToString(),
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList()
        };

        return stats;
    }
}
