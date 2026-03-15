using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.EstadosDispositivo.Commands;

public class UpdateEstadoDispositivoCommandHandler : IRequestHandler<UpdateEstadoDispositivoCommand, Result<EstadoDispositivoDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEstadoDispositivoCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EstadoDispositivoDto>> Handle(UpdateEstadoDispositivoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var estado = await _unitOfWork.EstadosDispositivo.GetById(request.EstadoDispositivoId);
            if (estado == null)
                return Result<EstadoDispositivoDto>.Failure($"Estado con ID {request.EstadoDispositivoId} no encontrado.");

            estado.Descripcion = request.Descripcion;
            await _unitOfWork.EstadosDispositivo.UpdateAsync(estado, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<EstadoDispositivoDto>.Success(_mapper.Map<EstadoDispositivoDto>(estado));
        }
        catch (Exception ex)
        {
            return Result<EstadoDispositivoDto>.Failure($"Error al actualizar el estado: {ex.Message}");
        }
    }
}
