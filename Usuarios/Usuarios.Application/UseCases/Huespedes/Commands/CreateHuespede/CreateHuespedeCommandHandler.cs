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

    private const int TipoDocumentoCedula = 2;

    public async Task<HuespedeDto> Handle(CreateHuespedeCommand request, CancellationToken cancellationToken)
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

    /// <summary>
    /// Valida una cédula dominicana usando el algoritmo módulo 10.
    /// Multiplica los primeros 10 dígitos por [1,2,1,2,...], suma los dígitos de
    /// cada producto, y verifica que el dígito 11 sea (10 - (suma % 10)) % 10.
    /// </summary>
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
