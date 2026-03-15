using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.TiposDispositivo.Queries;

public class GetAllTiposDispositivoQuery : IRequest<Result<IEnumerable<TipoDispositivoDto>>> { }
