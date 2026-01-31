using MediatR;
using Usuarios.Application.DTOs.PermisosPersonal;

namespace Usuarios.Application.UseCases.PermisosPersonal.Commands.CreatePermiso;

public record CreatePermisoCommand(CreatePermisosPersonalDto Permiso) : IRequest<PermisosPersonalDto>;
