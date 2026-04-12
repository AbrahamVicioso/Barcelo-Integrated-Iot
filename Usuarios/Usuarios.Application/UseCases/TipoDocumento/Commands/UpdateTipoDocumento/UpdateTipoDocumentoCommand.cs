using MediatR;
using Usuarios.Application.DTOs.TipoDocumento;

namespace Usuarios.Application.UseCases.TipoDocumento.Commands.UpdateTipoDocumento;

public record UpdateTipoDocumentoCommand(UpdateTipoDocumentoDto TipoDocumento) : IRequest<TipoDocumentoDto>;
