using MediatR;
using Usuarios.Application.DTOs.PermisosPersonal;

namespace Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisosByPersonal;

public record GetPermisosByPersonalQuery(int PersonalId) : IRequest<IEnumerable<PermisosPersonalDto>>;
