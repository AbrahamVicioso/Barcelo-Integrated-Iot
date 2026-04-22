using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.EstadosReservaActividad.Queries;

public class GetAllEstadosReservaActividadQueryHandler : IRequestHandler<GetAllEstadosReservaActividadQuery, Result<IEnumerable<EstadoReservaActividadDto>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetAllEstadosReservaActividadQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<EstadoReservaActividadDto>>> Handle(GetAllEstadosReservaActividadQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var estados = await _unitOfWork.EstadosReservaActividad.GetAllAsync(cancellationToken);
            var estadosDto = _mapper.Map<IEnumerable<EstadoReservaActividadDto>>(estados);
            return Result<IEnumerable<EstadoReservaActividadDto>>.Success(estadosDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<EstadoReservaActividadDto>>.Failure($"Error al obtener los estados: {ex.Message}");
        }
    }
}