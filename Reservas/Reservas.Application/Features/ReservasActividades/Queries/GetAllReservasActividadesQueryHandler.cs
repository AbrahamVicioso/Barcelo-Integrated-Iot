using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Domain.Interfaces;

namespace Reservas.Application.Features.ReservasActividades.Queries;

public class GetAllReservasActividadesQueryHandler : IRequestHandler<GetAllReservasActividadesQuery, Result<IEnumerable<ReservaActividadDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllReservasActividadesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ReservaActividadDto>>> Handle(GetAllReservasActividadesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var reservas = await _unitOfWork.ReservasActividades.GetAllAsync(cancellationToken);
            var reservasDto = _mapper.Map<IEnumerable<ReservaActividadDto>>(reservas);
            return Result<IEnumerable<ReservaActividadDto>>.Success(reservasDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ReservaActividadDto>>.Failure($"Error al obtener las reservas de actividades: {ex.Message}");
        }
    }
}
