using MediatR;
using Usuarios.Application.DTOs.Personal;

namespace Usuarios.Application.UseCases.Personal.Queries.GetPersonalByDepartamento;

public record GetPersonalByDepartamentoQuery(int DepartamentoId) : IRequest<IEnumerable<PersonalDto>>;
