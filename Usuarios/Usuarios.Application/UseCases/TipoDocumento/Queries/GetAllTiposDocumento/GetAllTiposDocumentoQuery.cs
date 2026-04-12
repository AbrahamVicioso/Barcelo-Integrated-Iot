using MediatR;
using Usuarios.Application.DTOs.TipoDocumento;

namespace Usuarios.Application.UseCases.TipoDocumento.Queries.GetAllTiposDocumento;

public record GetAllTiposDocumentoQuery : IRequest<IEnumerable<TipoDocumentoDto>>;
