using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Personal;

namespace Usuarios.Application.UseCases.Personal.Queries.GetPersonalById;

public record GetPersonalByIdQuery(int PersonalId) : IRequest<Result<PersonalDto>>;
