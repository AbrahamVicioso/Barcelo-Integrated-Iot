using MediatR;
using Usuarios.Application.DTOs.PermisosPersonal;

namespace Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisosByHabitacion;

public record GetPermisosByHabitacionQuery(int HabitacionId) : IRequest<IEnumerable<PermisosPersonalDto>>;
