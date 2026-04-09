using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.TiposDispositivo.Queries;

public class GetAllTiposDispositivoQueryHandler : IRequestHandler<GetAllTiposDispositivoQuery, Result<PagedResult<TipoDispositivoDto>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetAllTiposDispositivoQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<TipoDispositivoDto>>> Handle(GetAllTiposDispositivoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var todos = await _unitOfWork.TiposDispositivo.GetAll();
            var todosDto = _mapper.Map<IEnumerable<TipoDispositivoDto>>(todos).ToList();
            var totalCount = todosDto.Count;
            var items = todosDto
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            return Result<PagedResult<TipoDispositivoDto>>.Success(
                new PagedResult<TipoDispositivoDto>(items, request.Page, request.PageSize, totalCount));
        }
        catch (Exception ex)
        {
            return Result<PagedResult<TipoDispositivoDto>>.Failure($"Error al obtener los tipos: {ex.Message}");
        }
    }
}
