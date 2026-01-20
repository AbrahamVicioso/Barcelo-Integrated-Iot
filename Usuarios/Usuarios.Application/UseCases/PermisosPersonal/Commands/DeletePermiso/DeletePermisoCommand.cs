using MediatR;
using Usuarios.Application.Common;

namespace Usuarios.Application.UseCases.PermisosPersonal.Commands.DeletePermiso;

public record DeletePermisoCommand(int PermisoId) : IRequest<Result<bool>>;
