using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Entities;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Huespedes.Commands.CreateHuespedeMe;

public class CreateHuespedeMeCommandHandler : IRequestHandler<CreateHuespedeMeCommand, HuespedeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuthenticationApiClient _authenticationApiClient;

    public CreateHuespedeMeCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuthenticationApiClient authenticationApiClient)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _authenticationApiClient = authenticationApiClient;
    }

    private const int TipoDocumentoCedula = 2;

    public async Task<HuespedeDto> Handle(CreateHuespedeMeCommand request, CancellationToken cancellationToken)
    {
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

        var existingByUsuario = await _unitOfWork.Huespedes.GetByUsuarioIdAsync(request.UsuarioId);

        var existingByDocumento = await _unitOfWork.Huespedes.GetByDocumentoAsync(
            request.Huespede.TipoDocumentoId,
            request.Huespede.NumeroDocumento);

        HuespedeDto huespedeDto;

        if (existingByUsuario != null)
        {
            if (existingByDocumento != null && existingByDocumento.HuespedId != existingByUsuario.HuespedId)
                throw new ConflictException("Ya existe un huésped con ese documento");

            existingByUsuario.NombreCompleto = request.Huespede.NombreCompleto;
            existingByUsuario.TipoDocumentoId = request.Huespede.TipoDocumentoId;
            existingByUsuario.NumeroDocumento = request.Huespede.NumeroDocumento;
            existingByUsuario.Nacionalidad = request.Huespede.Nacionalidad;
            existingByUsuario.FechaNacimiento = request.Huespede.FechaNacimiento;
            existingByUsuario.ContactoEmergencia = request.Huespede.ContactoEmergencia;
            existingByUsuario.TelefonoEmergencia = request.Huespede.TelefonoEmergencia;
            existingByUsuario.PreferenciasAlimentarias = request.Huespede.PreferenciasAlimentarias;
            existingByUsuario.NotasEspeciales = request.Huespede.NotasEspeciales;

            await _unitOfWork.Huespedes.UpdateAsync(existingByUsuario);
            await _unitOfWork.SaveChangesAsync();

            huespedeDto = _mapper.Map<HuespedeDto>(existingByUsuario);
        }
        else
        {
            if (existingByDocumento != null)
                throw new ConflictException("Ya existe un huésped con ese documento");

            var huespede = new Huespede
            {
                UsuarioId = request.UsuarioId,
                NombreCompleto = request.Huespede.NombreCompleto,
                TipoDocumentoId = request.Huespede.TipoDocumentoId,
                NumeroDocumento = request.Huespede.NumeroDocumento,
                Nacionalidad = request.Huespede.Nacionalidad,
                FechaNacimiento = request.Huespede.FechaNacimiento,
                ContactoEmergencia = request.Huespede.ContactoEmergencia,
                TelefonoEmergencia = request.Huespede.TelefonoEmergencia,
                PreferenciasAlimentarias = request.Huespede.PreferenciasAlimentarias,
                NotasEspeciales = request.Huespede.NotasEspeciales,
                EsVip = false,
                FechaRegistro = DateTime.Now
            };

            var createdHuespede = await _unitOfWork.Huespedes.AddAsync(huespede);
            await _unitOfWork.SaveChangesAsync();

            huespedeDto = _mapper.Map<HuespedeDto>(createdHuespede);
        }

        huespedeDto.NombreTipoDocumento = tipoDocumento.Nombre;
        huespedeDto.CorreoElectronico = await _authenticationApiClient.GetEmailByUserIdAsync(request.UsuarioId);

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
