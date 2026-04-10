using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.RegistrosAcceso.Queries;

public class GetRegistrosAccesoByUsuarioIdQueryHandler : IRequestHandler<GetRegistrosAccesoByUsuarioIdQuery, Result<PagedResult<RegistrosAccesoDto>>>
{
    private readonly IRegistrosAccesoRepository _registroRepository;
    private readonly IMapper _mapper;

    public GetRegistrosAccesoByUsuarioIdQueryHandler(IRegistrosAccesoRepository registroRepository, IMapper mapper)
    {
        _registroRepository = registroRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<RegistrosAccesoDto>>> Handle(GetRegistrosAccesoByUsuarioIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var registros = await _registroRepository.GetByUsuarioIdAsync(request.UsuarioId);
            var dtos = _mapper.Map<IEnumerable<RegistrosAccesoDto>>(registros).ToList();
            var totalCount = dtos.Count;
            var items = dtos
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            return Result<PagedResult<RegistrosAccesoDto>>.Success(
                new PagedResult<RegistrosAccesoDto>(items, request.Page, request.PageSize, totalCount));
        }
        catch (Exception ex)
        {
            return Result<PagedResult<RegistrosAccesoDto>>.Failure($"Error al obtener los registros de acceso: {ex.Message}");
        }
    }
}
