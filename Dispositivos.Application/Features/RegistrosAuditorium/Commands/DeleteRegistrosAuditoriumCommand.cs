using MediatR;
using Dispositivos.Application.Common;

namespace Dispositivos.Application.Features.RegistrosAuditorium.Commands;

public class DeleteRegistrosAuditoriumCommand : IRequest<Result<int>>
{
    public int RegistroId { get; set; }
}
