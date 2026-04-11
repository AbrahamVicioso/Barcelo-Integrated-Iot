using MediatR;
using Usuarios.Application.DTOs.Puesto;

namespace Usuarios.Application.UseCases.Puesto.Commands.UpdatePuesto;

public record UpdatePuestoCommand(UpdatePuestoDto Puesto) : IRequest<PuestoDto>;
