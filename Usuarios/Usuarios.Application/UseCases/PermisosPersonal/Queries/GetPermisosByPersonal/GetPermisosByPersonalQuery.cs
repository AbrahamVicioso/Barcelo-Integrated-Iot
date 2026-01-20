using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.PermisosPersonal;

namespace Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisosByPersonal;

public record GetPermisosByPersonalQuery(int PersonalId) : IRequest<Result<IEnumerable<PermisosPersonalDto>>>;
