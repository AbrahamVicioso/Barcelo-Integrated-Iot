using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Domain.Entities;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Huespedes.Commands.CreateHuespede;

public class CreateHuespedeCommandHandler : IRequestHandler<CreateHuespedeCommand, HuespedeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuthenticationApiClient _authenticationApiClient;

    public CreateHuespedeCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuthenticationApiClient authenticationApiClient)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _authenticationApiClient = authenticationApiClient;
    }

    public async Task<HuespedeDto> Handle(CreateHuespedeCommand request, CancellationToken cancellationToken)
    {
        var usuarioId = await _authenticationApiClient.GetUserIdByEmailAsync(request.Huespede.CorreoElectronico);
        if (usuarioId == null)
        {
            throw new Exception("No se encontró un usuario con ese correo electrónico");
        }

        var existingByUsuario = await _unitOfWork.Huespedes.GetByUsuarioIdAsync(usuarioId.ToString()!);
        if (existingByUsuario != null)
        {
            throw new Exception("Ya existe un huésped asociado a ese correo electrónico");
        }

        var existingByDocumento = await _unitOfWork.Huespedes.GetByDocumentoAsync(
            request.Huespede.TipoDocumento,
            request.Huespede.NumeroDocumento);
        if (existingByDocumento != null)
        {
            throw new Exception("Ya existe un huésped con ese documento");
        }

        var huespede = _mapper.Map<Huespede>(request.Huespede);
        huespede.UsuarioId = usuarioId.ToString()!;
        huespede.FechaRegistro = DateTime.UtcNow;

        var createdHuespede = await _unitOfWork.Huespedes.AddAsync(huespede);

        await _unitOfWork.SaveChangesAsync();

        var huespedeDto = _mapper.Map<HuespedeDto>(createdHuespede);

        return huespedeDto;
    }
}
