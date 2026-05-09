using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<UpdateDispositivoCommandHandler> _logger;

    public UpdateDispositivoCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ITbDeviceService tbDeviceService,
        ILogger<UpdateDispositivoCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tbDeviceService = tbDeviceService;
        _logger = logger;
    }

    public async Task<Result<DispositivoDto>> Handle(UpdateDispositivoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = request.Dispositivo;

            // Verificar que el dispositivo existe
            var dispositivo = await _unitOfWork.Dispositivos.GetById(request.DispositivoId);
            if (dispositivo == null)
                return Result<DispositivoDto>.NotFound($"Dispositivo con ID {request.DispositivoId} no encontrado.");

            // CHECK: NivelBateria 0-100
            if (dto.NivelBateria < 0 || dto.NivelBateria > 100)
                return Result<DispositivoDto>.Failure("El nivel de batería debe estar entre 0 y 100.");

            // FK: TipoDispositivoId debe existir
            var tipo = await _unitOfWork.TiposDispositivo.GetById(dto.TipoDispositivoId);
            if (tipo == null)
                return Result<DispositivoDto>.Failure($"El tipo de dispositivo con ID {dto.TipoDispositivoId} no existe.");

            // FK: EstadoDispositivoId debe existir
            var estado = await _unitOfWork.EstadosDispositivo.GetById(dto.EstadoDispositivoId);
            if (estado == null)
                return Result<DispositivoDto>.Failure($"El estado de dispositivo con ID {dto.EstadoDispositivoId} no existe.");

            // UNIQUE: NumeroSerieDispositivo — excluir el propio registro
            var existenteSerie = await _unitOfWork.Dispositivos.GetByNumeroSerie(dto.NumeroSerieDispositivo);
            if (existenteSerie != null && existenteSerie.DispositivoId != request.DispositivoId)
                return Result<DispositivoDto>.Failure($"Ya existe otro dispositivo con el número de serie '{dto.NumeroSerieDispositivo}'.");

            // UNIQUE: DireccionMAC — excluir el propio registro
            var existenteMAC = await _unitOfWork.Dispositivos.GetByDireccionMAC(dto.DireccionMac);
            if (existenteMAC != null && existenteMAC.DispositivoId != request.DispositivoId)
                return Result<DispositivoDto>.Failure($"Ya existe otro dispositivo con la dirección MAC '{dto.DireccionMac}'.");

            // UNIQUE: IpDispositivo — excluir el propio registro (si se envía)
            if (!string.IsNullOrWhiteSpace(dto.Ipdispositivo))
            {
                var existenteIp = await _unitOfWork.Dispositivos.GetByIpDispositivo(dto.Ipdispositivo);
                if (existenteIp != null && existenteIp.DispositivoId != request.DispositivoId)
                    return Result<DispositivoDto>.Failure($"Ya existe otro dispositivo con la dirección IP '{dto.Ipdispositivo}'.");
            }

            _mapper.Map(dto, dispositivo);
            dispositivo.EstadoDispositivo = null;
            dispositivo.TipoDispositivo = null;
            dispositivo.CerradurasInteligentes = [];
            dispositivo.MantenimientoCerraduras = [];
            dispositivo.FechaActualizacion = DateTime.Now;

            await _unitOfWork.Dispositivos.UpdateAsync(dispositivo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Sincronizar con ThingsBoard (el nombre del device = DispositivoId)
            try
            {
                var tbDevice = await _tbDeviceService.GetDeviceByNameAsync(
                    dispositivo.DispositivoId.ToString(), cancellationToken);

                if (tbDevice != null && !string.IsNullOrEmpty(tbDevice.Id))
                {
                    await _tbDeviceService.UpdateDeviceAsync(
                        tbDevice.Id,
                        dispositivo.DispositivoId.ToString(),
                        tipo.Nombre,
                        dispositivo.NumeroSerieDispositivo,
                        cancellationToken);
                }
                else
                {
                    await _tbDeviceService.CreateOrUpdateDeviceAsync(
                        null,
                        dispositivo.DispositivoId.ToString(),
                        tipo.Nombre,
                        dispositivo.NumeroSerieDispositivo,
                        null,
                        cancellationToken);
                }
            }
            catch (HttpRequestException ex)
            {
                // DB update already succeeded — log warning but don't fail the request
                _logger.LogWarning(ex,
                    "Dispositivo {DispositivoId} actualizado en DB pero falló sincronización con ThingsBoard",
                    dispositivo.DispositivoId);
            }

            var dispositivoDto = _mapper.Map<DispositivoDto>(dispositivo);
            return Result<DispositivoDto>.Success(dispositivoDto);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            if (inner.Contains("UQ_Dispositivos_NumeroSerie"))
                return Result<DispositivoDto>.Failure($"Ya existe otro dispositivo con el número de serie '{request.Dispositivo.NumeroSerieDispositivo}'.");
            if (inner.Contains("UQ_Dispositivos_MAC"))
                return Result<DispositivoDto>.Failure($"Ya existe otro dispositivo con la dirección MAC '{request.Dispositivo.DireccionMac}'.");
            if (inner.Contains("UQ_Dispositivos_IP"))
                return Result<DispositivoDto>.Failure($"Ya existe otro dispositivo con la dirección IP '{request.Dispositivo.Ipdispositivo}'.");
            if (inner.Contains("FK_Dispositivos_TiposDispositivo"))
                return Result<DispositivoDto>.Failure($"El tipo de dispositivo con ID {request.Dispositivo.TipoDispositivoId} no existe.");
            if (inner.Contains("FK_Dispositivos_EstadosDispositivo"))
                return Result<DispositivoDto>.Failure($"El estado de dispositivo con ID {request.Dispositivo.EstadoDispositivoId} no existe.");
            if (inner.Contains("FK_Dispositivos_Hoteles"))
                return Result<DispositivoDto>.Failure($"El hotel con ID {request.Dispositivo.HotelId} no existe.");
            if (inner.Contains("CHK_Dispositivos_Bateria"))
                return Result<DispositivoDto>.Failure("El nivel de batería debe estar entre 0 y 100.");
            return Result<DispositivoDto>.Failure($"Error de base de datos: {inner}");
        }
        catch (Exception ex)
        {
            return Result<DispositivoDto>.Failure($"Error inesperado al actualizar el dispositivo: {ex.Message}");
        }
    }
}
