using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.Dispositivos.Queries;

public class GetDispositivosByHotelIdQuery : IRequest<Result<IEnumerable<DispositivoDto>>>
{
    public int HotelId { get; set; }
}
