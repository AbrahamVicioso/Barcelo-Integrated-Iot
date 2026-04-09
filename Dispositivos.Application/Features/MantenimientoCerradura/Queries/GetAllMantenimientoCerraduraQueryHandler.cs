using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.MantenimientoCerradura.Queries;

public class GetAllMantenimientoCerraduraQueryHandler : IRequestHandler<GetAllMantenimientoCerraduraQuery, Result<PagedResult<MantenimientoCerraduraDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMantenimientoCerraduraRepository _mantenimientoRepository;

    public GetAllMantenimientoCerraduraQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IMantenimientoCerraduraRepository mantenimientoRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _mantenimientoRepository = mantenimientoRepository;
    }

    public async Task<Result<PagedResult<MantenimientoCerraduraDto>>> Handle(GetAllMantenimientoCerraduraQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var todos = await _mantenimientoRepository.GetAll();
            var todosDto = _mapper.Map<IEnumerable<MantenimientoCerraduraDto>>(todos).ToList();
            var totalCount = todosDto.Count;
            var items = todosDto
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            return Result<PagedResult<MantenimientoCerraduraDto>>.Success(
                new PagedResult<MantenimientoCerraduraDto>(items, request.Page, request.PageSize, totalCount));
        }
        catch (Exception ex)
        {
            return Result<PagedResult<MantenimientoCerraduraDto>>.Failure($"Error al obtener los mantenimientos de cerradura: {ex.Message}");
        }
    }
}
