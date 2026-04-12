using MediatR;
using Usuarios.Application.DTOs.TipoDocumento;

namespace Usuarios.Application.UseCases.TipoDocumento.Commands.CreateTipoDocumento;

public record CreateTipoDocumentoCommand(CreateTipoDocumentoDto TipoDocumento) : IRequest<TipoDocumentoDto>;
