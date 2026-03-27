using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedeById;

public class GetHuespedeByIdQueryHandler : IRequestHandler<GetHuespedeByIdQuery, HuespedeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuthenticationApiClient _authenticationApiClient;

    public GetHuespedeByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IAuthenticationApiClient authenticationApiClient)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _authenticationApiClient = authenticationApiClient;
    }

    public async Task<HuespedeDto> Handle(GetHuespedeByIdQuery request, CancellationToken cancellationToken)
    {
        var huespede = await _unitOfWork.Huespedes.GetByIdAsync(request.HuespedId);
        if (huespede == null)
        {
            throw new NotFoundException("Huésped no encontrado");
        }

        var huespedeDto = _mapper.Map<HuespedeDto>(huespede);
        huespedeDto.CorreoElectronico = await _authenticationApiClient.GetEmailByUserIdAsync(huespede.UsuarioId);
        return huespedeDto;
    }
}
