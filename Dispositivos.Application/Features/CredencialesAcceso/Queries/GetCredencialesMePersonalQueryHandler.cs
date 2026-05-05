using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.CredencialesAcceso.Queries;

public class GetCredencialesMePersonalQueryHandler : IRequestHandler<GetCredencialesMePersonalQuery, Result<PagedResult<CredencialesAccesoDto>>>
{
    private readonly ICredencialesAccesoRepository _credencialRepository;
    private readonly IUsuariosGrpcService _usuariosGrpcService;
    private readonly IMapper _mapper;

    public GetCredencialesMePersonalQueryHandler(
        ICredencialesAccesoRepository credencialRepository,
        IUsuariosGrpcService usuariosGrpcService,
        IMapper mapper)
    {
        _credencialRepository = credencialRepository;
        _usuariosGrpcService = usuariosGrpcService;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<CredencialesAccesoDto>>> Handle(GetCredencialesMePersonalQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var personalId = await _usuariosGrpcService.GetPersonalIdByUsuarioIdAsync(request.UsuarioId, cancellationToken);
            if (personalId is null)
                return Result<PagedResult<CredencialesAccesoDto>>.Failure("El usuario autenticado no tiene un perfil de personal activo.");

            var todos = await _credencialRepository.GetByPersonalId(personalId.Value);
            var todosDto = _mapper.Map<IEnumerable<CredencialesAccesoDto>>(todos).ToList();
            var items = todosDto
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return Result<PagedResult<CredencialesAccesoDto>>.Success(
                new PagedResult<CredencialesAccesoDto>(items, request.Page, request.PageSize, todosDto.Count));
        }
        catch (Exception ex)
        {
            return Result<PagedResult<CredencialesAccesoDto>>.Failure($"Error al obtener las credenciales de personal: {ex.Message}");
        }
    }
}
