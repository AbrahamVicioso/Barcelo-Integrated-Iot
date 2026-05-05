using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.CredencialesAcceso.Commands;

public class ToggleCredencialMeHuespedCommandHandler : IRequestHandler<ToggleCredencialMeHuespedCommand, Result<CredencialesAccesoDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICredencialesAccesoRepository _credencialRepository;
    private readonly IUsuariosGrpcService _usuariosGrpcService;
    private readonly IServiceScopeFactory _scopeFactory;

    public ToggleCredencialMeHuespedCommandHandler(
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

    public async Task<Result<CredencialesAccesoDto>> Handle(ToggleCredencialMeHuespedCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var huespedId = await _usuariosGrpcService.GetHuespedIdByUsuarioIdAsync(request.UsuarioId, cancellationToken);
            if (huespedId is null)
                return Result<CredencialesAccesoDto>.Failure("El usuario autenticado no tiene un perfil de huésped.");

            var credencial = await _credencialRepository.GetById(request.CredencialId);
            if (credencial is null)
                return Result<CredencialesAccesoDto>.NotFound($"Credencial con ID {request.CredencialId} no encontrada.");

            if (credencial.HuespedId != huespedId)
                return Result<CredencialesAccesoDto>.Failure("Esta credencial no pertenece a tu perfil de huésped.");

            credencial.EstaActiva = !credencial.EstaActiva;

            await _credencialRepository.UpdateAsync(credencial, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            using var scope = _scopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<ITbCredencialesSyncService>();
            await syncService.SyncByHuespedIdAsync(huespedId.Value, cancellationToken);

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
