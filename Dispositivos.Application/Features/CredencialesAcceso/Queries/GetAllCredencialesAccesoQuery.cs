using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.CredencialesAcceso.Queries;

public class GetAllCredencialesAccesoQuery : IRequest<Result<IEnumerable<CredencialesAccesoDto>>>
{
}
