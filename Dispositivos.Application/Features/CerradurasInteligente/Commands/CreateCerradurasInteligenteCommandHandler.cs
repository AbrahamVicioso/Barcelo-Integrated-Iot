using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using CerraduraEntity = Dispositivos.Domain.Entities.CerradurasInteligente;

namespace Dispositivos.Application.Features.CerradurasInteligente.Commands;

public class CreateCerradurasInteligenteCommandHandler : IRequestHandler<CreateCerradurasInteligenteCommand, Result<int>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICerradurasInteligenteRepository _cerraduraRepository;

    public CreateCerradurasInteligenteCommandHandler(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ICerradurasInteligenteRepository cerraduraRepository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _cerraduraRepository = cerraduraRepository;
    }

    public async Task<Result<int>> Handle(CreateCerradurasInteligenteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var hab = request.Cerradura.HabitacionId;
            var act = request.Cerradura.ActividadId;

            // Una cerradura debe pertenecer exactamente a una habitación O una actividad
            if (hab == null && act == null)
                return Result<int>.Failure("Debe especificar HabitacionId o ActividadId.");
            if (hab != null && act != null)
                return Result<int>.Failure("Una cerradura solo puede pertenecer a una habitación o a una actividad, no a ambas.");

            // Validar que el DispositivoId existe y no tenga ya una cerradura asignada
            var dispositivo = await _unitOfWork.Dispositivos.GetById(request.Cerradura.DispositivoId);
            if (dispositivo == null)
                return Result<int>.Failure($"Dispositivo con ID {request.Cerradura.DispositivoId} no encontrado.");

            var cerradurasDispositivo = await _cerraduraRepository.GetByDispositivoId(request.Cerradura.DispositivoId);
            if (cerradurasDispositivo.Any())
                return Result<int>.Failure($"El dispositivo '{dispositivo.NumeroSerieDispositivo}' ya tiene una cerradura asignada.");

            if (hab != null)
            {
                var existentes = await _cerraduraRepository.GetByHabitacionId(hab.Value);
                if (existentes.Any())
                    return Result<int>.Failure($"La habitación {hab.Value} ya tiene una cerradura asignada.");
            }
            else
            {
                var existente = await _cerraduraRepository.GetByActividadIdAsync(act!.Value, cancellationToken);
                if (existente != null)
                    return Result<int>.Failure($"La actividad {act.Value} ya tiene una cerradura asignada.");
            }

            var cerradura = _mapper.Map<CerraduraEntity>(request.Cerradura);

            await _cerraduraRepository.AddAsync(cerradura, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(cerradura.CerraduraId);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            if (inner.Contains("UQ_Cerraduras_Habitacion"))
                return Result<int>.Failure($"La habitación {request.Cerradura.HabitacionId} ya tiene una cerradura asignada.");
            if (inner.Contains("UQ_Cerraduras_Actividad"))
                return Result<int>.Failure($"La actividad {request.Cerradura.ActividadId} ya tiene una cerradura asignada.");
            if (inner.Contains("FK_Cerraduras_Dispositivos"))
                return Result<int>.Failure($"Dispositivo con ID {request.Cerradura.DispositivoId} no encontrado.");
            if (inner.Contains("FK_Cerraduras_Habitaciones"))
                return Result<int>.Failure($"Habitación con ID {request.Cerradura.HabitacionId} no encontrada.");
            if (inner.Contains("FK_Cerraduras_Actividades"))
                return Result<int>.Failure($"Actividad con ID {request.Cerradura.ActividadId} no encontrada.");
            if (inner.Contains("CHK_Cerraduras_Contexto"))
                return Result<int>.Failure("Debe especificar HabitacionId o ActividadId.");
            return Result<int>.Failure($"Error de base de datos al crear la cerradura: {inner}");
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al crear la cerradura inteligente: {ex.Message}");
        }
    }
}
