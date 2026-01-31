using MediatR;
using Usuarios.Application.DTOs.Personal;

namespace Usuarios.Application.UseCases.Personal.Queries.GetAllPersonal;

public record GetAllPersonalQuery : IRequest<IEnumerable<PersonalDto>>;
