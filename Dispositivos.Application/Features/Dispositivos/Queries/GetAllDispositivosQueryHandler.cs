using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.Dispositivos.Queries;

public class GetAllDispositivosQueryHandler : IRequestHandler<GetAllDispositivosQuery, Result<PagedResult<DispositivoDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllDispositivosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<DispositivoDto>>> Handle(GetAllDispositivosQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var todos = await _unitOfWork.Dispositivos.GetAll();
            var todosDto = _mapper.Map<IEnumerable<DispositivoDto>>(todos).ToList();
            var totalCount = todosDto.Count;
            var items = todosDto
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            return Result<PagedResult<DispositivoDto>>.Success(
                new PagedResult<DispositivoDto>(items, request.Page, request.PageSize, totalCount));
        }
        catch (Exception ex)
        {
            return Result<PagedResult<DispositivoDto>>.Failure($"Error al obtener los dispositivos: {ex.Message}");
        }
    }
}
