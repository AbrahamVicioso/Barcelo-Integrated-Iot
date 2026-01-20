using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.PermisosPersonal;

namespace Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisosActivos;

public record GetPermisosActivosQuery(int PersonalId) : IRequest<Result<IEnumerable<PermisosPersonalDto>>>;
