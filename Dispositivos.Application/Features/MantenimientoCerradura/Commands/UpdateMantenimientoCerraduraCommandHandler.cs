using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using MantenimientoEntity = Dispositivos.Domain.Entities.MantenimientoCerradura;

namespace Dispositivos.Application.Features.MantenimientoCerradura.Commands;

public class UpdateMantenimientoCerraduraCommandHandler : IRequestHandler<UpdateMantenimientoCerraduraCommand, Result<MantenimientoCerraduraDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMantenimientoCerraduraRepository _mantenimientoRepository;

    public UpdateMantenimientoCerraduraCommandHandler(
        IMapper mapper, 
        IUnitOfWork unitOfWork,
        IMantenimientoCerraduraRepository mantenimientoRepository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _mantenimientoRepository = mantenimientoRepository;
    }

    public async Task<Result<MantenimientoCerraduraDto>> Handle(UpdateMantenimientoCerraduraCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var mantenimiento = await _mantenimientoRepository.GetById(request.Mantenimiento.MantenimientoId);

            if (mantenimiento == null)
            {
                return Result<MantenimientoCerraduraDto>.Failure($"Mantenimiento con ID {request.Mantenimiento.MantenimientoId} no encontrado.");
            }

            _mapper.Map(request.Mantenimiento, mantenimiento);
            
            await _mantenimientoRepository.UpdateAsync(mantenimiento, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var mantenimientoDto = _mapper.Map<MantenimientoCerraduraDto>(mantenimiento);
            return Result<MantenimientoCerraduraDto>.Success(mantenimientoDto);
        }
        catch (Exception ex)
        {
            return Result<MantenimientoCerraduraDto>.Failure($"Error al actualizar el mantenimiento de cerradura: {ex.Message}");
        }
    }
}
