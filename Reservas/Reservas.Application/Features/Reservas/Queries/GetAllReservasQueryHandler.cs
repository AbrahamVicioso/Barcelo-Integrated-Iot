using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Reservas.Queries;

public class GetAllReservasQueryHandler : IRequestHandler<GetAllReservasQuery, Result<IEnumerable<ReservaDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllReservasQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ReservaDto>>> Handle(GetAllReservasQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var reservas = await _unitOfWork.Reservas.GetAllAsync(cancellationToken);
            var reservasDto = _mapper.Map<IEnumerable<ReservaDto>>(reservas);
            return Result<IEnumerable<ReservaDto>>.Success(reservasDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ReservaDto>>.Failure($"Error al obtener las reservas: {ex.Message}");
        }
    }
}
