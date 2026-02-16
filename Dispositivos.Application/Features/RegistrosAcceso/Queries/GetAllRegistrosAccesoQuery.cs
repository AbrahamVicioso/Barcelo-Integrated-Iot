using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.RegistrosAcceso.Queries;

public class GetAllRegistrosAccesoQuery : IRequest<Result<IEnumerable<RegistrosAccesoDto>>>
{
}
