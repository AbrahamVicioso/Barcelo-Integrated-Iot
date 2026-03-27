using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Huespedes;
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
        var usuarioId = await _authenticationApiClient.GetUserIdByEmailAsync(request.Huespede.CorreoElectronico);
        if (usuarioId == null)
        {
            throw new Exception("No se encontró un usuario con ese correo electrónico");
        }

        var huespede = await _unitOfWork.Huespedes.GetByUsuarioIdAsync(usuarioId.ToString()!);
        if (huespede == null)
        {
            throw new Exception("Huésped no encontrado para ese correo electrónico");
        }

        huespede.NombreCompleto = request.Huespede.NombreCompleto;
        huespede.ContactoEmergencia = request.Huespede.ContactoEmergencia;
        huespede.TelefonoEmergencia = request.Huespede.TelefonoEmergencia;
        huespede.EsVip = request.Huespede.EsVip;
        huespede.PreferenciasAlimentarias = request.Huespede.PreferenciasAlimentarias;
        huespede.NotasEspeciales = request.Huespede.NotasEspeciales;

        await _unitOfWork.Huespedes.UpdateAsync(huespede);
        await _unitOfWork.SaveChangesAsync();

        var huespedeDto = _mapper.Map<HuespedeDto>(huespede);
        return huespedeDto;
    }
}
