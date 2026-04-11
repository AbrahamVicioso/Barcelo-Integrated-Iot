using MediatR;
using Usuarios.Application.DTOs.Puesto;

namespace Usuarios.Application.UseCases.Puesto.Commands.CreatePuesto;

public record CreatePuestoCommand(CreatePuestoDto Puesto) : IRequest<PuestoDto>;
