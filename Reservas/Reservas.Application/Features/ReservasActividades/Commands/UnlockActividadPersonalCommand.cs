using MediatR;
using Reservas.Application.Common;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public record UnlockActividadPersonalCommand(int ActividadId) : IRequest<Result<string>>;
