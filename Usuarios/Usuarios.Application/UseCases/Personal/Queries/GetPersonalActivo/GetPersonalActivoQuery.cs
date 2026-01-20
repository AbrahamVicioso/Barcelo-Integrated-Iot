using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Personal;

namespace Usuarios.Application.UseCases.Personal.Queries.GetPersonalActivo;

public record GetPersonalActivoQuery : IRequest<Result<IEnumerable<PersonalDto>>>;
