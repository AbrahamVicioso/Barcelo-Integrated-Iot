using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.MantenimientoCerradura.Queries;

public class GetAllMantenimientoCerraduraQueryHandler : IRequestHandler<GetAllMantenimientoCerraduraQuery, Result<IEnumerable<MantenimientoCerraduraDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMantenimientoCerraduraRepository _mantenimientoRepository;

    public GetAllMantenimientoCerraduraQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        IMantenimientoCerraduraRepository mantenimientoRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _mantenimientoRepository = mantenimientoRepository;
    }

    public async Task<Result<IEnumerable<MantenimientoCerraduraDto>>> Handle(GetAllMantenimientoCerraduraQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var mantenimientos = await _mantenimientoRepository.GetAll();
            var mantenimientosDto = _mapper.Map<IEnumerable<MantenimientoCerraduraDto>>(mantenimientos);
            return Result<IEnumerable<MantenimientoCerraduraDto>>.Success(mantenimientosDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<MantenimientoCerraduraDto>>.Failure($"Error al obtener los mantenimientos de cerradura: {ex.Message}");
        }
    }
}
