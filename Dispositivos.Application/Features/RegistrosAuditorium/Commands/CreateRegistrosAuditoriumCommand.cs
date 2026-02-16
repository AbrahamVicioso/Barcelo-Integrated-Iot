using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.RegistrosAuditorium.Commands;

public class CreateRegistrosAuditoriumCommand : IRequest<Result<int>>
{
    public CreateRegistrosAuditoriumDto Registro { get; set; } = new();
}
