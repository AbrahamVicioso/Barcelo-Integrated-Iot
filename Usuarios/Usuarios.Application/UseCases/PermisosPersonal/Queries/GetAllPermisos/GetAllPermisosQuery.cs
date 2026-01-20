using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.PermisosPersonal;

namespace Usuarios.Application.UseCases.PermisosPersonal.Queries.GetAllPermisos;

public record GetAllPermisosQuery : IRequest<Result<IEnumerable<PermisosPersonalDto>>>;
