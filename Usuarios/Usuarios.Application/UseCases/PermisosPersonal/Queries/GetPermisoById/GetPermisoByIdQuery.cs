using MediatR;
using Usuarios.Application.DTOs.PermisosPersonal;

namespace Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisoById;

public record GetPermisoByIdQuery(int PermisoId) : IRequest<PermisosPersonalDto>;
