using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.CredencialesAcceso.Commands;

public class UpdateCredencialesAccesoCommand : IRequest<Result<CredencialesAccesoDto>>
{
    public UpdateCredencialesAccesoDto Credencial { get; set; } = new();
}
