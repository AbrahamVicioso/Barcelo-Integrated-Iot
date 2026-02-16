using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.CredencialesAcceso.Queries;

public class GetCredencialesAccesoByIdQuery : IRequest<Result<CredencialesAccesoDto>>
{
    public int CredencialId { get; set; }
}
