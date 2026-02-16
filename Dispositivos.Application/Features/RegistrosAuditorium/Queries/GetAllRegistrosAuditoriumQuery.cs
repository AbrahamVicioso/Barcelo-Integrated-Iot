using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.RegistrosAuditorium.Queries;

public class GetAllRegistrosAuditoriumQuery : IRequest<Result<IEnumerable<RegistrosAuditoriumDto>>>
{
}
