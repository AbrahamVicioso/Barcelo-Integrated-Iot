using MediatR;
using Usuarios.Application.DTOs.PermisosPersonal;

namespace Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisosActivos;

public record GetPermisosActivosQuery(int PersonalId) : IRequest<IEnumerable<PermisosPersonalDto>>;
