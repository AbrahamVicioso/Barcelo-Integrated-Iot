using MediatR;

namespace Usuarios.Application.UseCases.Puesto.Commands.DeletePuesto;

public record DeletePuestoCommand(int PuestoId) : IRequest<bool>;
