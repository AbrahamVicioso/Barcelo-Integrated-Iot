using MediatR;
using Usuarios.Application.DTOs.Huespedes;

namespace Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedeById;

public record GetHuespedeByIdQuery(int HuespedId) : IRequest<HuespedeDto>;
