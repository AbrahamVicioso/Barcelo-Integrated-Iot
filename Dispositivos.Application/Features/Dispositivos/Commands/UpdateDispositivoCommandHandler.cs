using System.Net;
using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Features.Dispositivos.Commands;

public class UpdateDispositivoCommandHandler : IRequestHandler<UpdateDispositivoCommand, Result<DispositivoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITbDeviceService _tbDeviceService;

    public UpdateDispositivoCommandHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        ITbDeviceService tbDeviceService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tbDeviceService = tbDeviceService;
    }

    public async Task<Result<DispositivoDto>> Handle(UpdateDispositivoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dispositivo = await _unitOfWork.Dispositivos.GetById(request.DispositivoId);

            if (dispositivo == null)
            {
                return Result<DispositivoDto>.Failure($"Dispositivo con ID {request.DispositivoId} no encontrado.");
            }

            // Store old type for comparison
            var oldTipoDispositivo = dispositivo.TipoDispositivo;

            _mapper.Map(request.Dispositivo, dispositivo);
            dispositivo.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Dispositivos.UpdateAsync(dispositivo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Sync with Thingsboard if device exists
            if (!string.IsNullOrEmpty(dispositivo.NumeroSerieDispositivo))
            {
                try
                {
                    // Use the device ID as the Thingsboard identifier
                    await _tbDeviceService.UpdateDeviceAsync(
                        dispositivo.DispositivoId.ToString(),
                        dispositivo.DispositivoId.ToString(),
                        dispositivo.TipoDispositivo,
                        dispositivo.NumeroSerieDispositivo,
                        cancellationToken);
                }
                catch (HttpRequestException ex)
                {
                    // If device doesn't exist in Thingsboard by ID (404), try to find by name and update
                    if (ex.Message.Contains("404") || ex.Message.Contains("NotFound"))
                    {
                        try
                        {
                            // Try to find device by name
                            var existingDevice = await _tbDeviceService.GetDeviceByNameAsync(
                                dispositivo.DispositivoId.ToString(),
                                cancellationToken);

                            if (existingDevice != null)
                            {
                                // Device exists with different ID - update using found ID
                                await _tbDeviceService.UpdateDeviceAsync(
                                    existingDevice.Id,
                                    dispositivo.DispositivoId.ToString(),
                                    dispositivo.TipoDispositivo,
                                    dispositivo.NumeroSerieDispositivo,
                                    cancellationToken);
                            }
                            else
                            {
                                // Device doesn't exist at all - create it
                                await _tbDeviceService.CreateOrUpdateDeviceAsync(
                                    null,
                                    dispositivo.DispositivoId.ToString(),
                                    dispositivo.TipoDispositivo,
                                    dispositivo.NumeroSerieDispositivo,
                                    dispositivo.DispositivoId.ToString(),
                                    cancellationToken);
                            }
                        }
                        catch (HttpRequestException findEx)
                        {
                            return Result<DispositivoDto>.Failure(
                                $"Dispositivo actualizado localmente pero falló la sincronización con Thingsboard: {findEx.Message}");
                        }
                    }
                    // If device name already exists (400), find by name and update
                    else if (ex.Message.Contains("name already exists") || ex.Message.Contains("400"))
                    {
                        try
                        {
                            var existingDevice = await _tbDeviceService.GetDeviceByNameAsync(
                                dispositivo.DispositivoId.ToString(),
                                cancellationToken);

                            if (existingDevice != null)
                            {
                                await _tbDeviceService.UpdateDeviceAsync(
                                    existingDevice.Id,
                                    dispositivo.DispositivoId.ToString(),
                                    dispositivo.TipoDispositivo,
                                    dispositivo.NumeroSerieDispositivo,
                                    cancellationToken);
                            }
                        }
                        catch (HttpRequestException findEx)
                        {
                            return Result<DispositivoDto>.Failure(
                                $"Dispositivo actualizado localmente pero falló la sincronización con Thingsboard: {findEx.Message}");
                        }
                    }
                    else
                    {
                        return Result<DispositivoDto>.Failure(
                            $"Dispositivo actualizado localmente pero falló la sincronización con Thingsboard: {ex.Message}");
                    }
                }
            }

            var dispositivoDto = _mapper.Map<DispositivoDto>(dispositivo);
            return Result<DispositivoDto>.Success(dispositivoDto);
        }
        catch (Exception ex)
        {
            return Result<DispositivoDto>.Failure($"Error al actualizar el dispositivo: {ex.Message}");
        }
    }
}
