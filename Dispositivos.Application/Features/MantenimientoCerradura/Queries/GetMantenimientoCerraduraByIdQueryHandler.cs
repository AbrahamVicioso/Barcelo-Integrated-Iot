using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.MantenimientoCerradura.Queries;

public class GetMantenimientoCerraduraByIdQueryHandler : IRequestHandler<GetMantenimientoCerraduraByIdQuery, Result<MantenimientoCerraduraDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMantenimientoCerraduraRepository _mantenimientoRepository;

    public GetMantenimientoCerraduraByIdQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        IMantenimientoCerraduraRepository mantenimientoRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _mantenimientoRepository = mantenimientoRepository;
    }

    public async Task<Result<MantenimientoCerraduraDto>> Handle(GetMantenimientoCerraduraByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var mantenimiento = await _mantenimientoRepository.GetById(request.MantenimientoId);

            if (mantenimiento == null)
            {
                return Result<MantenimientoCerraduraDto>.Failure($"Mantenimiento con ID {request.MantenimientoId} no encontrado.");
            }

            var mantenimientoDto = _mapper.Map<MantenimientoCerraduraDto>(mantenimiento);
            return Result<MantenimientoCerraduraDto>.Success(mantenimientoDto);
        }
        catch (Exception ex)
        {
            return Result<MantenimientoCerraduraDto>.Failure($"Error al obtener el mantenimiento de cerradura: {ex.Message}");
        }
    }
}
