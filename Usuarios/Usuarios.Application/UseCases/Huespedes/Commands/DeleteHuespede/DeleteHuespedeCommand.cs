using MediatR;

namespace Usuarios.Application.UseCases.Huespedes.Commands.DeleteHuespede;

public record DeleteHuespedeCommand(int HuespedId) : IRequest<bool>;
