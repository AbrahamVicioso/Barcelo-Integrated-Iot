using MediatR;
using Usuarios.Application.DTOs.Departamento;

namespace Usuarios.Application.UseCases.Departamento.Queries.GetDepartamentoById;

public record GetDepartamentoByIdQuery(int DepartamentoId) : IRequest<DepartamentoDto>;
