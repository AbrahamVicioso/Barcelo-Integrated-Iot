using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Huespedes;

namespace Usuarios.Application.UseCases.Huespedes.Queries.GetAllHuespedes;

public record GetAllHuespedesQuery : IRequest<Result<IEnumerable<HuespedeDto>>>;
