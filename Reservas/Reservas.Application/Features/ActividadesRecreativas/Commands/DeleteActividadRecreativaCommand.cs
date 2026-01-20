using MediatR;
using Reservas.Application.Common;

namespace Reservas.Application.Features.ActividadesRecreativas.Commands;

public class DeleteActividadRecreativaCommand : IRequest<Result<bool>>
{
    public int ActividadId { get; set; }
}
