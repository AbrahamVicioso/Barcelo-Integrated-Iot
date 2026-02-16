using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.RegistrosAuditorium.Queries;

public class GetRegistrosAuditoriumByIdQuery : IRequest<Result<RegistrosAuditoriumDto>>
{
    public int RegistroId { get; set; }
}
