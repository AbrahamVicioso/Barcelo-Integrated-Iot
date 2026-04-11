using MediatR;

namespace Usuarios.Application.UseCases.Departamento.Commands.DeleteDepartamento;

public record DeleteDepartamentoCommand(int DepartamentoId) : IRequest<bool>;
