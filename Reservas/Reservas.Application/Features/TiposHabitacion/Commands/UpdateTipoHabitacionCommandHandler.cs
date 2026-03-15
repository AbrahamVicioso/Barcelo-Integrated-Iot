using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.TiposHabitacion.Commands;

public class UpdateTipoHabitacionCommandHandler : IRequestHandler<UpdateTipoHabitacionCommand, Result<TipoHabitacionDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTipoHabitacionCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TipoHabitacionDto>> Handle(UpdateTipoHabitacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tipo = await _unitOfWork.TiposHabitacion.GetById(request.TipoHabitacionId);
            if (tipo == null)
                return Result<TipoHabitacionDto>.Failure($"Tipo de habitacion con ID {request.TipoHabitacionId} no encontrado.");

            _mapper.Map(request, tipo);
            await _unitOfWork.TiposHabitacion.UpdateAsync(tipo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<TipoHabitacionDto>.Success(_mapper.Map<TipoHabitacionDto>(tipo));
        }
        catch (Exception ex)
        {
            return Result<TipoHabitacionDto>.Failure($"Error al actualizar el tipo de habitacion: {ex.Message}");
        }
    }
}
