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
                return Result<CerradurasInteligenteDto>.NotFound($"Cerradura con ID {request.Cerradura.CerraduraId} no encontrada.");

            // Validar que el DispositivoId existe y no esté en uso por otra cerradura
            if (request.Cerradura.DispositivoId != cerradura.DispositivoId)
            {
                var dispositivo = await _unitOfWork.Dispositivos.GetById(request.Cerradura.DispositivoId);
                if (dispositivo == null)
                    return Result<CerradurasInteligenteDto>.Failure($"Dispositivo con ID {request.Cerradura.DispositivoId} no encontrado.");

                var cerradurasDispositivo = await _cerraduraRepository.GetByDispositivoId(request.Cerradura.DispositivoId);
                if (cerradurasDispositivo.Any(c => c.CerraduraId != request.Cerradura.CerraduraId))
                    return Result<CerradurasInteligenteDto>.Failure($"El dispositivo '{dispositivo.NumeroSerieDispositivo}' ya tiene una cerradura asignada.");
            }

            // Validar que el HabitacionId no esté en uso por otra cerradura
            if (request.Cerradura.HabitacionId != cerradura.HabitacionId)
            {
                var cerradurasHabitacion = await _cerraduraRepository.GetByHabitacionId(request.Cerradura.HabitacionId);
                if (cerradurasHabitacion.Any())
                    return Result<CerradurasInteligenteDto>.Failure($"La habitación {request.Cerradura.HabitacionId} ya tiene una cerradura asignada.");
            }

            _mapper.Map(request.Cerradura, cerradura);
            cerradura.Dispositivo = null; // evita conflicto entre FK y navigation property al hacer Update con AsNoTracking

            await _cerraduraRepository.UpdateAsync(cerradura, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var cerraduraDto = _mapper.Map<CerradurasInteligenteDto>(cerradura);
            return Result<CerradurasInteligenteDto>.Success(cerraduraDto);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            if (inner.Contains("UQ_Cerraduras_Habitacion"))
                return Result<CerradurasInteligenteDto>.Failure($"La habitación {request.Cerradura.HabitacionId} ya tiene una cerradura asignada.");
            if (inner.Contains("FK_Cerraduras_Dispositivos"))
                return Result<CerradurasInteligenteDto>.Failure($"Dispositivo con ID {request.Cerradura.DispositivoId} no encontrado.");
            return Result<CerradurasInteligenteDto>.Failure($"Error de base de datos al actualizar la cerradura: {inner}");
        }
        catch (Exception ex)
        {
            return Result<CerradurasInteligenteDto>.Failure($"Error al actualizar la cerradura inteligente: {ex.Message}");
        }
    }
}
