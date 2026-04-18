using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using CredencialEntity = Dispositivos.Domain.Entities.CredencialesAcceso;
using System.Security.Cryptography;
using System.Text;

namespace Dispositivos.Application.Features.CredencialesAcceso.Commands;

public class UpdateCredencialesAccesoCommandHandler : IRequestHandler<UpdateCredencialesAccesoCommand, Result<CredencialesAccesoDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICredencialesAccesoRepository _credencialRepository;
    private readonly IServiceScopeFactory _scopeFactory;

    public UpdateCredencialesAccesoCommandHandler(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ICredencialesAccesoRepository credencialRepository,
        IServiceScopeFactory scopeFactory)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _credencialRepository = credencialRepository;
        _scopeFactory = scopeFactory;
    }

    private static string GenerarHash(string texto)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(texto);
        return Convert.ToBase64String(sha256.ComputeHash(bytes));
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

            var oldReservaId = credencial.ReservaId;
            var oldPersonalId = credencial.PersonalId;
            var oldHuespedId = credencial.HuespedId;
            var oldPin = credencial.CodigoPin;

            _mapper.Map(request.Credencial, credencial);

            // Regenerar hash si el PIN cambió
            if (credencial.CodigoPin != oldPin)
                credencial.HashPin = GenerarHash(credencial.CodigoPin);

            await _credencialRepository.UpdateAsync(credencial, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Sync ThingsBoard: cerraduras del usuario anterior y nuevo (si cambió)
            await TriggerSyncAsync(oldPersonalId, oldHuespedId, oldReservaId, cancellationToken);
            if (request.Credencial.PersonalId != oldPersonalId || request.Credencial.HuespedId != oldHuespedId || request.Credencial.ReservaId != oldReservaId)
                await TriggerSyncAsync(request.Credencial.PersonalId, request.Credencial.HuespedId, request.Credencial.ReservaId, cancellationToken);

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

    private async Task TriggerSyncAsync(int? personalId, int? huespedId, int? reservaId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var syncService = scope.ServiceProvider.GetRequiredService<ITbCredencialesSyncService>();
        if (personalId.HasValue)
            await syncService.SyncByPersonalIdAsync(personalId.Value, cancellationToken);
        else if (huespedId.HasValue)
            await syncService.SyncByHuespedIdAsync(huespedId.Value, cancellationToken);
        else if (reservaId.HasValue)
            await syncService.SyncByReservaIdAsync(reservaId.Value, cancellationToken);
    }
}
