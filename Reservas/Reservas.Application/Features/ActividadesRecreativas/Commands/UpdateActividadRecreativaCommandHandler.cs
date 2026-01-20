using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Domain.Interfaces;

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
        catch (Exception ex)
        {
            return Result<ActividadRecreativaDto>.Failure($"Error al actualizar la actividad: {ex.Message}");
        }
    }
}
