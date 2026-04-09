using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.RegistrosAuditorium.Queries;

public class GetAllRegistrosAuditoriumQueryHandler : IRequestHandler<GetAllRegistrosAuditoriumQuery, Result<PagedResult<RegistrosAuditoriumDto>>>
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

    public async Task<Result<PagedResult<RegistrosAuditoriumDto>>> Handle(GetAllRegistrosAuditoriumQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var todos = await _registroRepository.GetAllAsync();
            var todosDto = _mapper.Map<IEnumerable<RegistrosAuditoriumDto>>(todos).ToList();
            var totalCount = todosDto.Count;
            var items = todosDto
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            return Result<PagedResult<RegistrosAuditoriumDto>>.Success(
                new PagedResult<RegistrosAuditoriumDto>(items, request.Page, request.PageSize, totalCount));
        }
        catch (Exception ex)
        {
            return Result<PagedResult<RegistrosAuditoriumDto>>.Failure($"Error al obtener los registros de auditoría: {ex.Message}");
        }
    }
}
