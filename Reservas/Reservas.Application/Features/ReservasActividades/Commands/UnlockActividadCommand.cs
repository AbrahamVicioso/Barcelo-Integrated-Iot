using MediatR;
using Reservas.Application.Common;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public record UnlockActividadCommand(int ActividadId, string? Pin) : IRequest<Result<string>>;
