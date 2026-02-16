using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.RegistrosAuditorium.Queries;

public class GetAllRegistrosAuditoriumQueryHandler : IRequestHandler<GetAllRegistrosAuditoriumQuery, Result<IEnumerable<RegistrosAuditoriumDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IRegistrosAuditoriumRepository _registroRepository;

    public GetAllRegistrosAuditoriumQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        IRegistrosAuditoriumRepository registroRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _registroRepository = registroRepository;
    }

    public async Task<Result<IEnumerable<RegistrosAuditoriumDto>>> Handle(GetAllRegistrosAuditoriumQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var registros = await _registroRepository.GetAllAsync();
            var registrosDto = _mapper.Map<IEnumerable<RegistrosAuditoriumDto>>(registros);
            return Result<IEnumerable<RegistrosAuditoriumDto>>.Success(registrosDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<RegistrosAuditoriumDto>>.Failure($"Error al obtener los registros de auditoría: {ex.Message}");
        }
    }
}
