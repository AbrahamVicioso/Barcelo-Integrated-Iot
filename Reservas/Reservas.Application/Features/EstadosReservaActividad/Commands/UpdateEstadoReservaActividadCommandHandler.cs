using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.EstadosReservaActividad.Commands;

public class UpdateEstadoReservaActividadCommandHandler : IRequestHandler<UpdateEstadoReservaActividadCommand, Result<EstadoReservaActividadDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEstadoReservaActividadCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EstadoReservaActividadDto>> Handle(UpdateEstadoReservaActividadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _unitOfWork.EstadosReservaActividad.GetByIdAsync(request.EstadoReservaActividadId, cancellationToken);
            if (entity == null)
                return Result<EstadoReservaActividadDto>.NotFound($"Estado con ID {request.EstadoReservaActividadId} no encontrado.");

            entity.Nombre = request.Nombre;
            entity.Descripcion = request.Descripcion;

            await _unitOfWork.EstadosReservaActividad.UpdateAsync(entity, cancellationToken);

            var dto = _mapper.Map<EstadoReservaActividadDto>(entity);
            return Result<EstadoReservaActividadDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<EstadoReservaActividadDto>.Failure($"Error al actualizar el estado: {ex.Message}");
        }
    }
}