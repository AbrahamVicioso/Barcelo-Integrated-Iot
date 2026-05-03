using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly IServiceScopeFactory _scopeFactory;

    public UpdateCerradurasInteligenteCommandHandler(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ICerradurasInteligenteRepository cerraduraRepository,
        IServiceScopeFactory scopeFactory)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _cerraduraRepository = cerraduraRepository;
        _scopeFactory = scopeFactory;
    }

    public async Task<Result<CerradurasInteligenteDto>> Handle(UpdateCerradurasInteligenteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var hab = request.Cerradura.HabitacionId;
            var act = request.Cerradura.ActividadId;

            // Una cerradura debe pertenecer exactamente a una habitación O una actividad
            if (hab == null && act == null)
                return Result<CerradurasInteligenteDto>.Failure("Debe especificar HabitacionId o ActividadId.");
            if (hab != null && act != null)
                return Result<CerradurasInteligenteDto>.Failure("Una cerradura solo puede pertenecer a una habitación o a una actividad, no a ambas.");

            var cerradura = await _cerraduraRepository.GetById(request.Cerradura.CerraduraId);
            if (cerradura == null)
                return Result<CerradurasInteligenteDto>.NotFound($"Cerradura con ID {request.Cerradura.CerraduraId} no encontrada.");

            // Capturar asociación anterior para sync post-update
            var oldHabitacionId = cerradura.HabitacionId;
            var oldActividadId = cerradura.ActividadId;

            // Validar DispositivoId si cambió
            if (request.Cerradura.DispositivoId != cerradura.DispositivoId)
            {
                var dispositivo = await _unitOfWork.Dispositivos.GetById(request.Cerradura.DispositivoId);
                if (dispositivo == null)
                    return Result<CerradurasInteligenteDto>.Failure($"Dispositivo con ID {request.Cerradura.DispositivoId} no encontrado.");

                var cerradurasDispositivo = await _cerraduraRepository.GetByDispositivoId(request.Cerradura.DispositivoId);
                if (cerradurasDispositivo.Any(c => c.CerraduraId != request.Cerradura.CerraduraId))
                    return Result<CerradurasInteligenteDto>.Failure($"El dispositivo '{dispositivo.NumeroSerieDispositivo}' ya tiene una cerradura asignada.");
            }

            // Validar HabitacionId si se asigna a habitación y cambió
            if (hab != null && hab != cerradura.HabitacionId)
            {
                var existentes = await _cerraduraRepository.GetByHabitacionId(hab.Value);
                if (existentes.Any(c => c.CerraduraId != request.Cerradura.CerraduraId))
                    return Result<CerradurasInteligenteDto>.Failure($"La habitación {hab.Value} ya tiene una cerradura asignada.");
            }

            // Validar ActividadId si se asigna a actividad y cambió
            if (act != null && act != cerradura.ActividadId)
            {
                var existente = await _cerraduraRepository.GetByActividadIdAsync(act.Value, cancellationToken);
                if (existente != null && existente.CerraduraId != request.Cerradura.CerraduraId)
                    return Result<CerradurasInteligenteDto>.Failure($"La actividad {act.Value} ya tiene una cerradura asignada.");
            }

            _mapper.Map(request.Cerradura, cerradura);
            cerradura.Dispositivo = null; // evita conflicto entre FK y navigation property al hacer Update con AsNoTracking

            await _cerraduraRepository.UpdateAsync(cerradura, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Sync credenciales: vieja asociación + nueva asociación
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<ITbCredencialesSyncService>();

                // Sync vieja asociación (credenciales antiguas se limpian o recalculan)
                if (oldHabitacionId.HasValue && oldHabitacionId != request.Cerradura.HabitacionId)
                    await syncService.SyncAsync(oldHabitacionId.Value, cancellationToken);
                if (oldActividadId.HasValue && oldActividadId != request.Cerradura.ActividadId)
                    await syncService.SyncByActividadIdAsync(oldActividadId.Value, cancellationToken);

                // Sync nueva asociación
                if (request.Cerradura.HabitacionId.HasValue)
                    await syncService.SyncAsync(request.Cerradura.HabitacionId.Value, cancellationToken);
                else if (request.Cerradura.ActividadId.HasValue)
                    await syncService.SyncByActividadIdAsync(request.Cerradura.ActividadId.Value, cancellationToken);
            }
            catch
            {
                // Non-fatal: cerradura ya actualizada
            }

            var cerraduraDto = _mapper.Map<CerradurasInteligenteDto>(cerradura);
            return Result<CerradurasInteligenteDto>.Success(cerraduraDto);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            if (inner.Contains("UQ_Cerraduras_Habitacion"))
                return Result<CerradurasInteligenteDto>.Failure($"La habitación {request.Cerradura.HabitacionId} ya tiene una cerradura asignada.");
            if (inner.Contains("UQ_Cerraduras_Actividad"))
                return Result<CerradurasInteligenteDto>.Failure($"La actividad {request.Cerradura.ActividadId} ya tiene una cerradura asignada.");
            if (inner.Contains("FK_Cerraduras_Dispositivos"))
                return Result<CerradurasInteligenteDto>.Failure($"Dispositivo con ID {request.Cerradura.DispositivoId} no encontrado.");
            if (inner.Contains("FK_Cerraduras_Habitaciones"))
                return Result<CerradurasInteligenteDto>.Failure($"Habitación con ID {request.Cerradura.HabitacionId} no encontrada.");
            if (inner.Contains("FK_Cerraduras_Actividades"))
                return Result<CerradurasInteligenteDto>.Failure($"Actividad con ID {request.Cerradura.ActividadId} no encontrada.");
            if (inner.Contains("CHK_Cerraduras_Contexto"))
                return Result<CerradurasInteligenteDto>.Failure("Debe especificar HabitacionId o ActividadId.");
            return Result<CerradurasInteligenteDto>.Failure($"Error de base de datos al actualizar la cerradura: {inner}");
        }
        catch (Exception ex)
        {
            return Result<CerradurasInteligenteDto>.Failure($"Error al actualizar la cerradura inteligente: {ex.Message}");
        }
    }
}
