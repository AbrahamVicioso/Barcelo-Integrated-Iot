using MediatR;
using Usuarios.Application.Common;

namespace Usuarios.Application.UseCases.Personal.Commands.DeletePersonal;

public record DeletePersonalCommand(int PersonalId) : IRequest<Result<bool>>;
