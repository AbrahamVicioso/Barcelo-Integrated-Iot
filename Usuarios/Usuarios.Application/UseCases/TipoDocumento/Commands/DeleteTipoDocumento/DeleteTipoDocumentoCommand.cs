using MediatR;

namespace Usuarios.Application.UseCases.TipoDocumento.Commands.DeleteTipoDocumento;

public record DeleteTipoDocumentoCommand(int TipoDocumentoId) : IRequest<bool>;
