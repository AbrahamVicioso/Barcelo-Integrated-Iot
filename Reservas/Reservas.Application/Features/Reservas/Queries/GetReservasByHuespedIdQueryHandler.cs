using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Domain.Interfaces;

namespace Reservas.Application.Features.Reservas.Queries;

public class GetReservasByHuespedIdQueryHandler : IRequestHandler<GetReservasByHuespedIdQuery, Result<IEnumerable<ReservaDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetReservasByHuespedIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ReservaDto>>> Handle(GetReservasByHuespedIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var reservas = await _unitOfWork.Reservas.GetReservasByHuespedIdAsync(request.HuespedId, cancellationToken);
            var reservasDto = _mapper.Map<IEnumerable<ReservaDto>>(reservas);
            return Result<IEnumerable<ReservaDto>>.Success(reservasDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ReservaDto>>.Failure($"Error al obtener las reservas del hu\u00e9sped: {ex.Message}");
        }
    }
}
