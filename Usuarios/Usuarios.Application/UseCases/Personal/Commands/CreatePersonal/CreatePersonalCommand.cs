using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Personal;

namespace Usuarios.Application.UseCases.Personal.Commands.CreatePersonal;

public record CreatePersonalCommand(CreatePersonalDto Personal) : IRequest<Result<PersonalDto>>;
