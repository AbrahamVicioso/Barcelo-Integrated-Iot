using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Features.Dispositivos.Commands;

public class UpdateDispositivoCommandHandler : IRequestHandler<UpdateDispositivoCommand, Result<DispositivoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateDispositivoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DispositivoDto>> Handle(UpdateDispositivoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dispositivo = await _unitOfWork.Dispositivos.GetById(request.DispositivoId);

            if (dispositivo == null)
            {
                return Result<DispositivoDto>.Failure($"Dispositivo con ID {request.DispositivoId} no encontrado.");
            }

            _mapper.Map(request.Dispositivo, dispositivo);
            dispositivo.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Dispositivos.UpdateAsync(dispositivo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dispositivoDto = _mapper.Map<DispositivoDto>(dispositivo);
            return Result<DispositivoDto>.Success(dispositivoDto);
        }
        catch (Exception ex)
        {
            return Result<DispositivoDto>.Failure($"Error al actualizar el dispositivo: {ex.Message}");
        }
    }
}
