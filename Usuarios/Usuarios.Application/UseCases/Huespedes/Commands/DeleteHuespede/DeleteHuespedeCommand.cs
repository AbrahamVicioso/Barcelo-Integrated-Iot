using MediatR;
using Usuarios.Application.Common;

namespace Usuarios.Application.UseCases.Huespedes.Commands.DeleteHuespede;

public record DeleteHuespedeCommand(int HuespedId) : IRequest<Result<bool>>;
