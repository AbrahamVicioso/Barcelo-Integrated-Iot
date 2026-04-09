using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.EstadosDispositivo.Queries;

public class GetAllEstadosDispositivoQueryHandler : IRequestHandler<GetAllEstadosDispositivoQuery, Result<PagedResult<EstadoDispositivoDto>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetAllEstadosDispositivoQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<EstadoDispositivoDto>>> Handle(GetAllEstadosDispositivoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var todos = await _unitOfWork.EstadosDispositivo.GetAll();
            var todosDto = _mapper.Map<IEnumerable<EstadoDispositivoDto>>(todos).ToList();
            var totalCount = todosDto.Count;
            var items = todosDto
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            return Result<PagedResult<EstadoDispositivoDto>>.Success(
                new PagedResult<EstadoDispositivoDto>(items, request.Page, request.PageSize, totalCount));
        }
        catch (Exception ex)
        {
            return Result<PagedResult<EstadoDispositivoDto>>.Failure($"Error al obtener los estados: {ex.Message}");
        }
    }
}
