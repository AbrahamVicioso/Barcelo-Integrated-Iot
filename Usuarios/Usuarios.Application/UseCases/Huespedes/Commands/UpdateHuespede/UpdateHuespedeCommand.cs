using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Huespedes;

namespace Usuarios.Application.UseCases.Huespedes.Commands.UpdateHuespede;

public record UpdateHuespedeCommand(UpdateHuespedeDto Huespede) : IRequest<Result<HuespedeDto>>;
