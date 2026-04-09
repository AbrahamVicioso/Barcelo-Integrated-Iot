using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.RegistrosAcceso.Queries;

public class GetAllRegistrosAccesoQueryHandler : IRequestHandler<GetAllRegistrosAccesoQuery, Result<PagedResult<RegistrosAccesoDto>>>
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

    public async Task<Result<PagedResult<RegistrosAccesoDto>>> Handle(GetAllRegistrosAccesoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var todos = await _registroRepository.GetAllAsync();
            var todosDto = _mapper.Map<IEnumerable<RegistrosAccesoDto>>(todos).ToList();
            var totalCount = todosDto.Count;
            var items = todosDto
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
