using MediatR;
using Usuarios.Application.DTOs.Huespedes;

namespace Usuarios.Application.UseCases.Huespedes.Commands.UpdateHuespede;

public record UpdateHuespedeCommand(UpdateHuespedeDto Huespede) : IRequest<HuespedeDto>;
