using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Personal;

namespace Usuarios.Application.UseCases.Personal.Queries.GetAllPersonal;

public record GetAllPersonalQuery : IRequest<Result<IEnumerable<PersonalDto>>>;
