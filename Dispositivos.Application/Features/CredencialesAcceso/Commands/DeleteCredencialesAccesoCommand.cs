using MediatR;
using Dispositivos.Application.Common;

namespace Dispositivos.Application.Features.CredencialesAcceso.Commands;

public class DeleteCredencialesAccesoCommand : IRequest<Result<bool>>
{
    public int CredencialId { get; set; }
}
