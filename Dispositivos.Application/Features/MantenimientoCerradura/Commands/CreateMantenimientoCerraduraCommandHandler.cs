using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using MantenimientoEntity = Dispositivos.Domain.Entities.MantenimientoCerradura;

namespace Dispositivos.Application.Features.MantenimientoCerradura.Commands;

public class CreateMantenimientoCerraduraCommandHandler : IRequestHandler<CreateMantenimientoCerraduraCommand, Result<int>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMantenimientoCerraduraRepository _mantenimientoRepository;

    public CreateMantenimientoCerraduraCommandHandler(
        IMapper mapper, 
        IUnitOfWork unitOfWork,
        IMantenimientoCerraduraRepository mantenimientoRepository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _mantenimientoRepository = mantenimientoRepository;
    }

    public async Task<Result<int>> Handle(CreateMantenimientoCerraduraCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var mantenimiento = _mapper.Map<MantenimientoEntity>(request.Mantenimiento);
            mantenimiento.FechaCreacion = DateTime.UtcNow;
            
            await _mantenimientoRepository.AddAsync(mantenimiento, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(mantenimiento.MantenimientoId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al crear el mantenimiento de cerradura: {ex.Message}");
        }
    }
}
