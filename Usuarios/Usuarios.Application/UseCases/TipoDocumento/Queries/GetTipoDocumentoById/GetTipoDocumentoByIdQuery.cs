using MediatR;
using Usuarios.Application.DTOs.TipoDocumento;

namespace Usuarios.Application.UseCases.TipoDocumento.Queries.GetTipoDocumentoById;

public record GetTipoDocumentoByIdQuery(int TipoDocumentoId) : IRequest<TipoDocumentoDto>;
