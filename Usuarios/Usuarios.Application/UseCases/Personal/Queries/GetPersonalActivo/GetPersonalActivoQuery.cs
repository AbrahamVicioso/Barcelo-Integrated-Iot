using MediatR;
using Usuarios.Application.DTOs.Personal;

namespace Usuarios.Application.UseCases.Personal.Queries.GetPersonalActivo;

public record GetPersonalActivoQuery : IRequest<IEnumerable<PersonalDto>>;
