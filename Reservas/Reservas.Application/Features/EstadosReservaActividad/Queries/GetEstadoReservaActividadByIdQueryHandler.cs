using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.EstadosReservaActividad.Queries;

public class GetEstadoReservaActividadByIdQueryHandler : IRequestHandler<GetEstadoReservaActividadByIdQuery, Result<EstadoReservaActividadDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetEstadoReservaActividadByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EstadoReservaActividadDto>> Handle(GetEstadoReservaActividadByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var estado = await _unitOfWork.EstadosReservaActividad.GetByIdAsync(request.EstadoReservaActividadId, cancellationToken);
            if (estado == null)
                return Result<EstadoReservaActividadDto>.NotFound($"Estado con ID {request.EstadoReservaActividadId} no encontrado.");

            var estadoDto = _mapper.Map<EstadoReservaActividadDto>(estado);
            return Result<EstadoReservaActividadDto>.Success(estadoDto);
        }
        catch (Exception ex)
        {
            return Result<EstadoReservaActividadDto>.Failure($"Error al obtener el estado: {ex.Message}");
        }
    }
}