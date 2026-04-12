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

    private const int TipoDocumentoCedula = 2;

    public async Task<HuespedeDto> Handle(UpdateHuespedeCommand request, CancellationToken cancellationToken)
    {
        var huespede = await _unitOfWork.Huespedes.GetByIdAsync(request.Huespede.HuespedId);
        if (huespede == null)
            throw new NotFoundException("Huésped no encontrado");

        var tipoDocumento = await _unitOfWork.TiposDocumento.GetByIdAsync(request.Huespede.TipoDocumentoId);
        if (tipoDocumento == null)
            throw new NotFoundException($"Tipo de documento con ID {request.Huespede.TipoDocumentoId} no encontrado");

        if (request.Huespede.TipoDocumentoId == TipoDocumentoCedula)
        {
            var cedula = request.Huespede.NumeroDocumento?.Trim() ?? string.Empty;
            if (cedula.Length != 11 || !cedula.All(char.IsDigit))
                throw new BusinessException("La cédula dominicana debe tener exactamente 11 dígitos numéricos.");
            if (!EsCedulaDominicanaValida(cedula))
                throw new BusinessException("La cédula dominicana ingresada no es válida. El dígito verificador no coincide.");
        }

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

    private static bool EsCedulaDominicanaValida(string cedula)
    {
        int[] pesos = [1, 2, 1, 2, 1, 2, 1, 2, 1, 2];
        int suma = 0;

        for (int i = 0; i < 10; i++)
        {
            int producto = (cedula[i] - '0') * pesos[i];
            suma += producto >= 10
                ? (producto / 10) + (producto % 10)
                : producto;
        }

        int digitoVerificador = (10 - (suma % 10)) % 10;
        return (cedula[10] - '0') == digitoVerificador;
    }
}
