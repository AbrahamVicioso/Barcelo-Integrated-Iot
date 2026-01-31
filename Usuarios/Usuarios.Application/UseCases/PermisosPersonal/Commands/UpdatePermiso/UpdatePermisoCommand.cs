using MediatR;
using Usuarios.Application.DTOs.PermisosPersonal;

namespace Usuarios.Application.UseCases.PermisosPersonal.Commands.UpdatePermiso;

public record UpdatePermisoCommand(UpdatePermisosPersonalDto Permiso) : IRequest<PermisosPersonalDto>;
