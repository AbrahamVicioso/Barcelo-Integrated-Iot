using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Huespedes.Queries.GetAllHuespedes;

public class GetAllHuespedesQueryHandler : IRequestHandler<GetAllHuespedesQuery, IEnumerable<HuespedeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuthenticationApiClient _authenticationApiClient;

    public GetAllHuespedesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IAuthenticationApiClient authenticationApiClient)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _authenticationApiClient = authenticationApiClient;
    }

    public async Task<IEnumerable<HuespedeDto>> Handle(GetAllHuespedesQuery request, CancellationToken cancellationToken)
    {
        var huespedes = await _unitOfWork.Huespedes.GetAllAsync();
        var huespedesDto = _mapper.Map<IEnumerable<HuespedeDto>>(huespedes).ToList();

        foreach (var (dto, entity) in huespedesDto.Zip(huespedes))
        {
            dto.CorreoElectronico = await _authenticationApiClient.GetEmailByUserIdAsync(entity.UsuarioId);
        }

        return huespedesDto;
    }
}
