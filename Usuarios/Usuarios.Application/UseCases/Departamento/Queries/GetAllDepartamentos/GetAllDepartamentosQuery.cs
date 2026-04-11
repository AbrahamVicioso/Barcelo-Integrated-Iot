using MediatR;
using Usuarios.Application.DTOs.Departamento;

namespace Usuarios.Application.UseCases.Departamento.Queries.GetAllDepartamentos;

public record GetAllDepartamentosQuery : IRequest<IEnumerable<DepartamentoDto>>;
