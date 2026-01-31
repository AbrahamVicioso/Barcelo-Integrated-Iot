using MediatR;
using Usuarios.Application.DTOs.PermisosPersonal;

namespace Usuarios.Application.UseCases.PermisosPersonal.Queries.GetAllPermisos;

public record GetAllPermisosQuery : IRequest<IEnumerable<PermisosPersonalDto>>;
