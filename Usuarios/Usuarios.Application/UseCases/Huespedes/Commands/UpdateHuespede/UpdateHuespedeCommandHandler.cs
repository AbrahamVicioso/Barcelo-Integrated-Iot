using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Huespedes.Commands.UpdateHuespede;

public class UpdateHuespedeCommandHandler : IRequestHandler<UpdateHuespedeCommand, HuespedeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuthenticationApiClient _authenticationApiClient;

    public UpdateHuespedeCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuthenticationApiClient authenticationApiClient)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _authenticationApiClient = authenticationApiClient;
    }

    public async Task<HuespedeDto> Handle(UpdateHuespedeCommand request, CancellationToken cancellationToken)
    {
        var huespede = await _unitOfWork.Huespedes.GetByIdAsync(request.Huespede.HuespedId);
        if (huespede == null)
            throw new NotFoundException("Huésped no encontrado");

        var tipoDocumento = await _unitOfWork.TiposDocumento.GetByIdAsync(request.Huespede.TipoDocumentoId);
        if (tipoDocumento == null)
            throw new NotFoundException($"Tipo de documento con ID {request.Huespede.TipoDocumentoId} no encontrado");

        var nuevoUsuarioId = await _authenticationApiClient.GetUserIdByEmailAsync(request.Huespede.CorreoElectronico);
        if (nuevoUsuarioId == null)
            throw new NotFoundException("No se encontró un usuario con ese correo electrónico");

        var nuevoUsuarioIdStr = nuevoUsuarioId.ToString()!;

        if (huespede.UsuarioId != nuevoUsuarioIdStr)
        {
            var existente = await _unitOfWork.Huespedes.GetByUsuarioIdAsync(nuevoUsuarioIdStr);
            if (existente != null)
                throw new ConflictException("Ese correo electrónico ya está asociado a otro huésped");

            huespede.UsuarioId = nuevoUsuarioIdStr;
        }

        var existenteDocumento = await _unitOfWork.Huespedes.GetByDocumentoAsync(
            request.Huespede.TipoDocumentoId,
            request.Huespede.NumeroDocumento);
        if (existenteDocumento != null && existenteDocumento.HuespedId != request.Huespede.HuespedId)
            throw new ConflictException("Ya existe un huésped con ese número de documento");

        huespede.NombreCompleto = request.Huespede.NombreCompleto;
        huespede.TipoDocumentoId = request.Huespede.TipoDocumentoId;
        huespede.NumeroDocumento = request.Huespede.NumeroDocumento;
        huespede.Nacionalidad = request.Huespede.Nacionalidad;
        huespede.FechaNacimiento = request.Huespede.FechaNacimiento;
        huespede.ContactoEmergencia = request.Huespede.ContactoEmergencia;
        huespede.TelefonoEmergencia = request.Huespede.TelefonoEmergencia;
        huespede.EsVip = request.Huespede.EsVip;
        huespede.PreferenciasAlimentarias = request.Huespede.PreferenciasAlimentarias;
        huespede.NotasEspeciales = request.Huespede.NotasEspeciales;

        await _unitOfWork.Huespedes.UpdateAsync(huespede);
        await _unitOfWork.SaveChangesAsync();

        var huespedeDto = _mapper.Map<HuespedeDto>(huespede);
        huespedeDto.NombreTipoDocumento = tipoDocumento.Nombre;

        return huespedeDto;
    }
}
