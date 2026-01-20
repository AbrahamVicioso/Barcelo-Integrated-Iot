using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Huespedes;

namespace Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedesVip;

public record GetHuespedesVipQuery : IRequest<Result<IEnumerable<HuespedeDto>>>;
