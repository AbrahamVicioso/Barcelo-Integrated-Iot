using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.CredencialesAcceso.Commands;

public class ToggleCredencialMePersonalCommandHandler : IRequestHandler<ToggleCredencialMePersonalCommand, Result<CredencialesAccesoDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICredencialesAccesoRepository _credencialRepository;
    private readonly IUsuariosGrpcService _usuariosGrpcService;
    private readonly IServiceScopeFactory _scopeFactory;

    public ToggleCredencialMePersonalCommandHandler(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ICredencialesAccesoRepository credencialRepository,
        IUsuariosGrpcService usuariosGrpcService,
        IServiceScopeFactory scopeFactory)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _credencialRepository = credencialRepository;
        _usuariosGrpcService = usuariosGrpcService;
        _scopeFactory = scopeFactory;
    }

    public async Task<Result<CredencialesAccesoDto>> Handle(ToggleCredencialMePersonalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var personalId = await _usuariosGrpcService.GetPersonalIdByUsuarioIdAsync(request.UsuarioId, cancellationToken);
            if (personalId is null)
                return Result<CredencialesAccesoDto>.Failure("El usuario autenticado no tiene un perfil de personal activo.");

            var credencial = await _credencialRepository.GetById(request.CredencialId);
            if (credencial is null)
                return Result<CredencialesAccesoDto>.NotFound($"Credencial con ID {request.CredencialId} no encontrada.");

            if (credencial.PersonalId != personalId)
                return Result<CredencialesAccesoDto>.Failure("Esta credencial no pertenece a tu perfil de personal.");

            credencial.EstaActiva = !credencial.EstaActiva;

            await _credencialRepository.UpdateAsync(credencial, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            using var scope = _scopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<ITbCredencialesSyncService>();
            await syncService.SyncByPersonalIdAsync(personalId.Value, cancellationToken);

            return Result<CredencialesAccesoDto>.Success(_mapper.Map<CredencialesAccesoDto>(credencial));
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            return Result<CredencialesAccesoDto>.Failure($"Error de base de datos al actualizar la credencial: {inner}");
        }
        catch (Exception ex)
        {
            return Result<CredencialesAccesoDto>.Failure($"Error al cambiar el estado de la credencial: {ex.Message}");
        }
    }
}
