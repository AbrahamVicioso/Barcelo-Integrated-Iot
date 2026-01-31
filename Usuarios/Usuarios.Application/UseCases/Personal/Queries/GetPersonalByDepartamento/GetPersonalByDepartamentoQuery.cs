using MediatR;
using Usuarios.Application.DTOs.Personal;

namespace Usuarios.Application.UseCases.Personal.Queries.GetPersonalByDepartamento;

public record GetPersonalByDepartamentoQuery(string Departamento) : IRequest<IEnumerable<PersonalDto>>;
