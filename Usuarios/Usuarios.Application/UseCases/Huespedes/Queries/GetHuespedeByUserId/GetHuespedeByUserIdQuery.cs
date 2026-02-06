using MediatR;
using Usuarios.Application.DTOs.Huespedes;

namespace Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedeByUserId;

public record GetHuespedeByUserIdQuery(string UsuarioId) : IRequest<HuespedeDto>;
