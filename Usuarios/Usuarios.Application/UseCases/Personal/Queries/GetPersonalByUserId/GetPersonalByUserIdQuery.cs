using MediatR;
using Usuarios.Application.DTOs.Personal;

namespace Usuarios.Application.UseCases.Personal.Queries.GetPersonalByUserId;

public record GetPersonalByUserIdQuery(string UsuarioId) : IRequest<PersonalDto>;
