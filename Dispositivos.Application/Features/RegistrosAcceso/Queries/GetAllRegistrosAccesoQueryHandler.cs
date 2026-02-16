using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.RegistrosAcceso.Queries;

public class GetAllRegistrosAccesoQueryHandler : IRequestHandler<GetAllRegistrosAccesoQuery, Result<IEnumerable<RegistrosAccesoDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IRegistrosAccesoRepository _registroRepository;

    public GetAllRegistrosAccesoQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        IRegistrosAccesoRepository registroRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _registroRepository = registroRepository;
    }

    public async Task<Result<IEnumerable<RegistrosAccesoDto>>> Handle(GetAllRegistrosAccesoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var registros = await _registroRepository.GetAllAsync();
            var registrosDto = _mapper.Map<IEnumerable<RegistrosAccesoDto>>(registros);
            return Result<IEnumerable<RegistrosAccesoDto>>.Success(registrosDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<RegistrosAccesoDto>>.Failure($"Error al obtener los registros de acceso: {ex.Message}");
        }
    }
}
