using MediatR;
using Usuarios.Application.DTOs.Puesto;

namespace Usuarios.Application.UseCases.Puesto.Queries.GetAllPuestos;

public record GetAllPuestosQuery : IRequest<IEnumerable<PuestoDto>>;
