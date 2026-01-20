using AutoMapper;
using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Huespedes.Commands.UpdateHuespede;

public class UpdateHuespedeCommandHandler : IRequestHandler<UpdateHuespedeCommand, Result<HuespedeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateHuespedeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<HuespedeDto>> Handle(UpdateHuespedeCommand request, CancellationToken cancellationToken)
    {
        var huespede = await _unitOfWork.Huespedes.GetByIdAsync(request.Huespede.HuespedId);
        if (huespede == null)
        {
            return Result<HuespedeDto>.Failure("Huésped no encontrado");
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
        return Result<HuespedeDto>.Success(huespedeDto, "Huésped actualizado exitosamente");
    }
}
