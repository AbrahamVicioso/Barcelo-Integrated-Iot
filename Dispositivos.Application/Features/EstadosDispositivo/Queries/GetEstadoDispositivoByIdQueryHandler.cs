using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.EstadosDispositivo.Queries;

public class GetEstadoDispositivoByIdQueryHandler : IRequestHandler<GetEstadoDispositivoByIdQuery, Result<EstadoDispositivoDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetEstadoDispositivoByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EstadoDispositivoDto>> Handle(GetEstadoDispositivoByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var estado = await _unitOfWork.EstadosDispositivo.GetById(request.EstadoDispositivoId);
            if (estado == null)
                return Result<EstadoDispositivoDto>.Failure($"Estado con ID {request.EstadoDispositivoId} no encontrado.");

            return Result<EstadoDispositivoDto>.Success(_mapper.Map<EstadoDispositivoDto>(estado));
        }
        catch (Exception ex)
        {
            return Result<EstadoDispositivoDto>.Failure($"Error al obtener el estado: {ex.Message}");
        }
    }
}
