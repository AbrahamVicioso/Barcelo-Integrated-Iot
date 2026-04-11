using MediatR;
using Usuarios.Application.DTOs.Departamento;

namespace Usuarios.Application.UseCases.Departamento.Commands.UpdateDepartamento;

public record UpdateDepartamentoCommand(UpdateDepartamentoDto Departamento) : IRequest<DepartamentoDto>;
