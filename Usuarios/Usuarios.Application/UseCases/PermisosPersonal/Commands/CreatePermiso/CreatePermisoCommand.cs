using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.PermisosPersonal;

namespace Usuarios.Application.UseCases.PermisosPersonal.Commands.CreatePermiso;

public record CreatePermisoCommand(CreatePermisosPersonalDto Permiso) : IRequest<Result<PermisosPersonalDto>>;
