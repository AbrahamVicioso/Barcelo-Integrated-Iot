using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.CredencialesAcceso.Commands;

public class CreateCredencialesAccesoCommand : IRequest<Result<int>>
{
    public CreateCredencialesAccesoDto Credencial { get; set; } = new();
}
