using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.ActividadesRecreativas.Commands;

public class UpdateActividadRecreativaCommandHandler : IRequestHandler<UpdateActividadRecreativaCommand, Result<ActividadRecreativaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateActividadRecreativaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ActividadRecreativaDto>> Handle(UpdateActividadRecreativaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var actividad = await _unitOfWork.ActividadesRecreativas.GetByIdAsync(request.ActividadId, cancellationToken);

            if (actividad == null)
            {
                return Result<ActividadRecreativaDto>.Failure($"Actividad con ID {request.ActividadId} no encontrada.");
            }

            _mapper.Map(request, actividad);

            await _unitOfWork.ActividadesRecreativas.UpdateAsync(actividad, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var actividadDto = _mapper.Map<ActividadRecreativaDto>(actividad);
            return Result<ActividadRecreativaDto>.Success(actividadDto);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            if (inner.Contains("CHK_Actividades_Horario"))
                return Result<ActividadRecreativaDto>.Failure("La hora de cierre debe ser posterior a la hora de apertura.");
            return Result<ActividadRecreativaDto>.Failure($"Error de base de datos al actualizar la actividad: {inner}");
        }
        catch (Exception ex)
        {
            return Result<ActividadRecreativaDto>.Failure($"Error al actualizar la actividad: {ex.Message}");
        }
    }
}
