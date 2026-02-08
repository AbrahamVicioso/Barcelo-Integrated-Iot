using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.Dispositivos.Queries;

public class GetDispositivoByIdQueryHandler : IRequestHandler<GetDispositivoByIdQuery, Result<DispositivoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDispositivoByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DispositivoDto>> Handle(GetDispositivoByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dispositivo = await _unitOfWork.Dispositivos.GetById(request.DispositivoId);

            if (dispositivo == null)
            {
                return Result<DispositivoDto>.Failure($"Dispositivo con ID {request.DispositivoId} no encontrado.");
            }

            var dispositivoDto = _mapper.Map<DispositivoDto>(dispositivo);
            return Result<DispositivoDto>.Success(dispositivoDto);
        }
        catch (Exception ex)
        {
            return Result<DispositivoDto>.Failure($"Error al obtener el dispositivo: {ex.Message}");
        }
    }
}
