using MediatR;
using Usuarios.Application.DTOs.Huespedes;

namespace Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedesVip;

public record GetHuespedesVipQuery : IRequest<IEnumerable<HuespedeDto>>;
