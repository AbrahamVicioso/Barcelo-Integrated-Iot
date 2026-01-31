using MediatR;

namespace Usuarios.Application.UseCases.Personal.Commands.DeletePersonal;

public record DeletePersonalCommand(int PersonalId) : IRequest<bool>;
