using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.RegistrosAcceso.Queries;

public class GetRegistrosAccesoByIdQuery : IRequest<Result<RegistrosAccesoDto>>
{
    public int RegistroId { get; set; }
}
