using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using CerraduraEntity = Dispositivos.Domain.Entities.CerradurasInteligente;

namespace Dispositivos.Application.Features.CerradurasInteligente.Commands;

public class UpdateCerradurasInteligenteCommandHandler : IRequestHandler<UpdateCerradurasInteligenteCommand, Result<CerradurasInteligenteDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICerradurasInteligenteRepository _cerraduraRepository;

    public UpdateCerradurasInteligenteCommandHandler(
        IMapper mapper, 
        IUnitOfWork unitOfWork,
        ICerradurasInteligenteRepository cerraduraRepository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _cerraduraRepository = cerraduraRepository;
    }

    public async Task<Result<CerradurasInteligenteDto>> Handle(UpdateCerradurasInteligenteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var cerradura = await _cerraduraRepository.GetById(request.Cerradura.CerraduraId);

            if (cerradura == null)
            {
                return Result<CerradurasInteligenteDto>.Failure($"Cerradura con ID {request.Cerradura.CerraduraId} no encontrada.");
            }

            _mapper.Map(request.Cerradura, cerradura);
            
            await _cerraduraRepository.UpdateAsync(cerradura, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var cerraduraDto = _mapper.Map<CerradurasInteligenteDto>(cerradura);
            return Result<CerradurasInteligenteDto>.Success(cerraduraDto);
        }
        catch (Exception ex)
        {
            return Result<CerradurasInteligenteDto>.Failure($"Error al actualizar la cerradura inteligente: {ex.Message}");
        }
    }
}
