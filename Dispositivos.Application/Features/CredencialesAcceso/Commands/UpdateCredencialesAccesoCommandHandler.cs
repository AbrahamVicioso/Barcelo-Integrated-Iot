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
                return Result<CredencialesAccesoDto>.NotFound($"Credencial con ID {request.Credencial.CredencialId} no encontrada.");

            if (request.Credencial.FechaExpiracion <= request.Credencial.FechaActivacion)
                return Result<CredencialesAccesoDto>.Failure("La fecha de expiración debe ser posterior a la fecha de activación.");

            _mapper.Map(request.Credencial, credencial);

            await _credencialRepository.UpdateAsync(credencial, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var credencialDto = _mapper.Map<CredencialesAccesoDto>(credencial);
            return Result<CredencialesAccesoDto>.Success(credencialDto);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            if (inner.Contains("CHK_Credenciales_Fechas"))
                return Result<CredencialesAccesoDto>.Failure("La fecha de expiración debe ser posterior a la fecha de activación.");
            return Result<CredencialesAccesoDto>.Failure($"Error de base de datos al actualizar la credencial: {inner}");
        }
        catch (Exception ex)
        {
            return Result<CredencialesAccesoDto>.Failure($"Error al actualizar la credencial de acceso: {ex.Message}");
        }
    }
}
