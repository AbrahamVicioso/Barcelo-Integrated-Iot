using MediatR;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;

namespace Reservas.Application.Features.Reservas.Queries;

public class GetAllEstadosReservaQueryHandler : IRequestHandler<GetAllEstadosReservaQuery, Result<IEnumerable<EstadoReserva>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllEstadosReservaQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<EstadoReserva>>> Handle(GetAllEstadosReservaQuery request, CancellationToken cancellationToken)
    {
        var estados = await _unitOfWork.EstadosReserva.GetAllAsync(cancellationToken);
        return Result<IEnumerable<EstadoReserva>>.Success(estados);
    }
}
