using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Application.Exceptions;
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
        var tipoDocumento = await _unitOfWork.TiposDocumento.GetByIdAsync(request.Huespede.TipoDocumentoId);
        if (tipoDocumento == null)
            throw new NotFoundException($"Tipo de documento con ID {request.Huespede.TipoDocumentoId} no encontrado");

        var usuarioId = await _authenticationApiClient.GetUserIdByEmailAsync(request.Huespede.CorreoElectronico);
        if (usuarioId == null)
            throw new NotFoundException("No se encontró un usuario con ese correo electrónico");

        var existingByUsuario = await _unitOfWork.Huespedes.GetByUsuarioIdAsync(usuarioId.ToString()!);
        if (existingByUsuario != null)
            throw new ConflictException("Ya existe un huésped asociado a ese correo electrónico");

        var existingByDocumento = await _unitOfWork.Huespedes.GetByDocumentoAsync(
            request.Huespede.TipoDocumentoId,
            request.Huespede.NumeroDocumento);
        if (existingByDocumento != null)
            throw new ConflictException("Ya existe un huésped con ese documento");

        var huespede = _mapper.Map<Huespede>(request.Huespede);
        huespede.UsuarioId = usuarioId.ToString()!;
        huespede.FechaRegistro = DateTime.UtcNow;

        var createdHuespede = await _unitOfWork.Huespedes.AddAsync(huespede);
        await _unitOfWork.SaveChangesAsync();

        var huespedeDto = _mapper.Map<HuespedeDto>(createdHuespede);
        huespedeDto.NombreTipoDocumento = tipoDocumento.Nombre;

        return huespedeDto;
    }
}
