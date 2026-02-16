using MediatR;
using Dispositivos.Application.Common;

namespace Dispositivos.Application.Features.RegistrosAcceso.Commands;

public class DeleteRegistrosAccesoCommand : IRequest<Result<int>>
{
    public int RegistroId { get; set; }
}
