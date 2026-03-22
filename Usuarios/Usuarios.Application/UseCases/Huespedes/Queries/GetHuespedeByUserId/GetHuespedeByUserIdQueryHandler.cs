using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedeByUserId;

public class GetHuespedeByUserIdQueryHandler : IRequestHandler<GetHuespedeByUserIdQuery, HuespedeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuthenticationApiClient _authenticationApiClient;

    public GetHuespedeByUserIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IAuthenticationApiClient authenticationApiClient)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _authenticationApiClient = authenticationApiClient;
    }

    public async Task<HuespedeDto> Handle(GetHuespedeByUserIdQuery request, CancellationToken cancellationToken)
    {
        var huespede = await _unitOfWork.Huespedes.GetByUsuarioIdAsync(request.UsuarioId);
        if (huespede == null)
        {
            throw new Exception("Huésped no encontrado para el usuario especificado");
        }

        var huespedeDto = _mapper.Map<HuespedeDto>(huespede);
        huespedeDto.CorreoElectronico = await _authenticationApiClient.GetEmailByUserIdAsync(huespede.UsuarioId);
        return huespedeDto;
    }
}
