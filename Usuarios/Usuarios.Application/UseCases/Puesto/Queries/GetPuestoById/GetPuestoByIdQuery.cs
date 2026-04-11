using MediatR;
using Usuarios.Application.DTOs.Puesto;

namespace Usuarios.Application.UseCases.Puesto.Queries.GetPuestoById;

public record GetPuestoByIdQuery(int PuestoId) : IRequest<PuestoDto>;
