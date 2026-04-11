using MediatR;
using Usuarios.Application.DTOs.PermisosPersonal;

namespace Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisosByActividad;

public record GetPermisosByActividadQuery(int ActividadId) : IRequest<IEnumerable<PermisosPersonalDto>>;
