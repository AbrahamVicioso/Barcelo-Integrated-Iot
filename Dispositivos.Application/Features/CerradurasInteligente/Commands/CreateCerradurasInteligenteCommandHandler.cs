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
            // Validar que el DispositivoId existe
            var dispositivo = await _unitOfWork.Dispositivos.GetById(request.Cerradura.DispositivoId);
            if (dispositivo == null)
                return Result<int>.Failure($"Dispositivo con ID {request.Cerradura.DispositivoId} no encontrado.");

            // Validar que el dispositivo no tenga ya una cerradura asignada
            var cerradurasDispositivo = await _cerraduraRepository.GetByDispositivoId(request.Cerradura.DispositivoId);
            if (cerradurasDispositivo.Any())
                return Result<int>.Failure($"El dispositivo '{dispositivo.NumeroSerieDispositivo}' ya tiene una cerradura asignada.");

            // Validar que la habitación no tenga ya una cerradura asignada
            var cerradurasHabitacion = await _cerraduraRepository.GetByHabitacionId(request.Cerradura.HabitacionId);
            if (cerradurasHabitacion.Any())
                return Result<int>.Failure($"La habitación {request.Cerradura.HabitacionId} ya tiene una cerradura asignada.");

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
            if (inner.Contains("FK_Cerraduras_Dispositivos"))
                return Result<int>.Failure($"Dispositivo con ID {request.Cerradura.DispositivoId} no encontrado.");
            return Result<int>.Failure($"Error de base de datos al crear la cerradura: {inner}");
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al crear la cerradura inteligente: {ex.Message}");
        }
    }
}
