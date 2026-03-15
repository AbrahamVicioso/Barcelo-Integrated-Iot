using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.EstadosDispositivo.Queries;

public class GetAllEstadosDispositivoQueryHandler : IRequestHandler<GetAllEstadosDispositivoQuery, Result<IEnumerable<EstadoDispositivoDto>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetAllEstadosDispositivoQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<EstadoDispositivoDto>>> Handle(GetAllEstadosDispositivoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var estados = await _unitOfWork.EstadosDispositivo.GetAll();
            var estadosDto = _mapper.Map<IEnumerable<EstadoDispositivoDto>>(estados);
            return Result<IEnumerable<EstadoDispositivoDto>>.Success(estadosDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<EstadoDispositivoDto>>.Failure($"Error al obtener los estados: {ex.Message}");
        }
    }
}
