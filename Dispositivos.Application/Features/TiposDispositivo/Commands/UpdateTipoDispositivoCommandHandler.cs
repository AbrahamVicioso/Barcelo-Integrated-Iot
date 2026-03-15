using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.TiposDispositivo.Commands;

public class UpdateTipoDispositivoCommandHandler : IRequestHandler<UpdateTipoDispositivoCommand, Result<TipoDispositivoDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTipoDispositivoCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TipoDispositivoDto>> Handle(UpdateTipoDispositivoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tipo = await _unitOfWork.TiposDispositivo.GetById(request.TipoDispositivoId);
            if (tipo == null)
                return Result<TipoDispositivoDto>.Failure($"Tipo con ID {request.TipoDispositivoId} no encontrado.");

            tipo.Nombre = request.Nombre;
            await _unitOfWork.TiposDispositivo.UpdateAsync(tipo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<TipoDispositivoDto>.Success(_mapper.Map<TipoDispositivoDto>(tipo));
        }
        catch (Exception ex)
        {
            return Result<TipoDispositivoDto>.Failure($"Error al actualizar el tipo: {ex.Message}");
        }
    }
}
