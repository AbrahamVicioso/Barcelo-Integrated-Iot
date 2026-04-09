using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.CerradurasInteligente.Queries;

public class GetAllCerradurasInteligenteQueryHandler : IRequestHandler<GetAllCerradurasInteligenteQuery, Result<PagedResult<CerradurasInteligenteDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICerradurasInteligenteRepository _cerraduraRepository;

    public GetAllCerradurasInteligenteQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICerradurasInteligenteRepository cerraduraRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cerraduraRepository = cerraduraRepository;
    }

    public async Task<Result<PagedResult<CerradurasInteligenteDto>>> Handle(GetAllCerradurasInteligenteQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var todos = await _cerraduraRepository.GetAll();
            var todosDto = _mapper.Map<IEnumerable<CerradurasInteligenteDto>>(todos).ToList();
            var totalCount = todosDto.Count;
            var items = todosDto
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            return Result<PagedResult<CerradurasInteligenteDto>>.Success(
                new PagedResult<CerradurasInteligenteDto>(items, request.Page, request.PageSize, totalCount));
        }
        catch (Exception ex)
        {
            return Result<PagedResult<CerradurasInteligenteDto>>.Failure($"Error al obtener las cerraduras inteligentes: {ex.Message}");
        }
    }
}
