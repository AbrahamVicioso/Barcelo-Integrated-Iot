using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.ActividadesRecreativas.Queries;

public class GetActividadByIdQuery : IRequest<Result<ActividadRecreativaDto>>
{
    public int ActividadId { get; set; }
}
