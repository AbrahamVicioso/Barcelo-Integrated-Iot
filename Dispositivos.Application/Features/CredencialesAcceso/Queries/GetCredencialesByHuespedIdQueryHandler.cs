using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.CredencialesAcceso.Queries;

public class GetCredencialesByHuespedIdQueryHandler : IRequestHandler<GetCredencialesByHuespedIdQuery, Result<PagedResult<CredencialesAccesoDto>>>
{
    private readonly ICredencialesAccesoRepository _credencialRepository;
    private readonly IMapper _mapper;

    public GetCredencialesByHuespedIdQueryHandler(
        ICredencialesAccesoRepository credencialRepository,
        IMapper mapper)
    {
        _credencialRepository = credencialRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<CredencialesAccesoDto>>> Handle(GetCredencialesByHuespedIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var todos = await _credencialRepository.GetByHuespedId(request.HuespedId);
            var todosDto = _mapper.Map<IEnumerable<CredencialesAccesoDto>>(todos).ToList();
            var totalCount = todosDto.Count;
            var items = todosDto
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            return Result<PagedResult<CredencialesAccesoDto>>.Success(
                new PagedResult<CredencialesAccesoDto>(items, request.Page, request.PageSize, totalCount));
        }
        catch (Exception ex)
        {
            return Result<PagedResult<CredencialesAccesoDto>>.Failure($"Error al obtener las credenciales del huésped: {ex.Message}");
        }
    }
}
