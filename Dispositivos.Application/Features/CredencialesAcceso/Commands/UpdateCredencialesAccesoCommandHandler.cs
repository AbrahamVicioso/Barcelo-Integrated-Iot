using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using CredencialEntity = Dispositivos.Domain.Entities.CredencialesAcceso;

namespace Dispositivos.Application.Features.CredencialesAcceso.Commands;

public class UpdateCredencialesAccesoCommandHandler : IRequestHandler<UpdateCredencialesAccesoCommand, Result<CredencialesAccesoDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICredencialesAccesoRepository _credencialRepository;

    public UpdateCredencialesAccesoCommandHandler(
        IMapper mapper, 
        IUnitOfWork unitOfWork,
        ICredencialesAccesoRepository credencialRepository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _credencialRepository = credencialRepository;
    }

    public async Task<Result<CredencialesAccesoDto>> Handle(UpdateCredencialesAccesoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var credencial = await _credencialRepository.GetById(request.Credencial.CredencialId);

            if (credencial == null)
            {
                return Result<CredencialesAccesoDto>.Failure($"Credencial con ID {request.Credencial.CredencialId} no encontrada.");
            }

            _mapper.Map(request.Credencial, credencial);
            
            await _credencialRepository.UpdateAsync(credencial, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var credencialDto = _mapper.Map<CredencialesAccesoDto>(credencial);
            return Result<CredencialesAccesoDto>.Success(credencialDto);
        }
        catch (Exception ex)
        {
            return Result<CredencialesAccesoDto>.Failure($"Error al actualizar la credencial de acceso: {ex.Message}");
        }
    }
}
