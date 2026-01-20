using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Personal;

namespace Usuarios.Application.UseCases.Personal.Commands.UpdatePersonal;

public record UpdatePersonalCommand(UpdatePersonalDto Personal) : IRequest<Result<PersonalDto>>;
