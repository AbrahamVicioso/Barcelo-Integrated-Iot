using MediatR;
using Usuarios.Application.DTOs.Huespedes;

namespace Usuarios.Application.UseCases.Huespedes.Queries.GetAllHuespedes;

public record GetAllHuespedesQuery : IRequest<IEnumerable<HuespedeDto>>;
