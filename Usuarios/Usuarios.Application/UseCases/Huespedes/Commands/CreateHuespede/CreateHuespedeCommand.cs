using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Huespedes;

namespace Usuarios.Application.UseCases.Huespedes.Commands.CreateHuespede;

public record CreateHuespedeCommand(CreateHuespedeDto Huespede) : IRequest<Result<HuespedeDto>>;
