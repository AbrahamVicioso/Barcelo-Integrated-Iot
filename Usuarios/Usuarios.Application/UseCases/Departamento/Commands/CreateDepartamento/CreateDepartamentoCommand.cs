using MediatR;
using Usuarios.Application.DTOs.Departamento;

namespace Usuarios.Application.UseCases.Departamento.Commands.CreateDepartamento;

public record CreateDepartamentoCommand(CreateDepartamentoDto Departamento) : IRequest<DepartamentoDto>;
