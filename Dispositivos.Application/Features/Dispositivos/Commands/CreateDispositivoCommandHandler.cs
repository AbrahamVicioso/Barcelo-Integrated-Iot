using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Features.Dispositivos.Commands;

public class CreateDispositivoCommandHandler : IRequestHandler<CreateDispositivoCommand, Result<Guid>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITbDeviceService _tbDeviceService;

    public CreateDispositivoCommandHandler(
        IMapper mapper, 
        IUnitOfWork unitOfWork,
        ITbDeviceService tbDeviceService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _tbDeviceService = tbDeviceService;
    }

    public async Task<Result<Guid>> Handle(CreateDispositivoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = request.Dispositivo;

            // CHECK: NivelBateria 0-100
            if (dto.NivelBateria < 0 || dto.NivelBateria > 100)
                return Result<Guid>.Failure("El nivel de batería debe estar entre 0 y 100.");

            // FK: TipoDispositivoId debe existir
            var tipo = await _unitOfWork.TiposDispositivo.GetById(dto.TipoDispositivoId);
            if (tipo == null)
                return Result<Guid>.Failure($"El tipo de dispositivo con ID {dto.TipoDispositivoId} no existe.");

            // FK: EstadoDispositivoId debe existir
            var estado = await _unitOfWork.EstadosDispositivo.GetById(dto.EstadoDispositivoId);
            if (estado == null)
                return Result<Guid>.Failure($"El estado de dispositivo con ID {dto.EstadoDispositivoId} no existe.");

            // UNIQUE: NumeroSerieDispositivo
            var existenteSerie = await _unitOfWork.Dispositivos.GetByNumeroSerie(dto.NumeroSerieDispositivo);
            if (existenteSerie != null)
                return Result<Guid>.Failure($"Ya existe un dispositivo con el número de serie '{dto.NumeroSerieDispositivo}'.");

            // UNIQUE: DireccionMac
            var existenteMAC = await _unitOfWork.Dispositivos.GetByDireccionMAC(dto.DireccionMac);
            if (existenteMAC != null)
                return Result<Guid>.Failure($"Ya existe un dispositivo con la dirección MAC '{dto.DireccionMac}'.");

            // UNIQUE: IpDispositivo (si se envía)
            if (!string.IsNullOrWhiteSpace(dto.Ipdispositivo))
            {
                var existenteIp = await _unitOfWork.Dispositivos.GetByIpDispositivo(dto.Ipdispositivo);
                if (existenteIp != null)
                    return Result<Guid>.Failure($"Ya existe un dispositivo con la dirección IP '{dto.Ipdispositivo}'.");
            }

            var dispositivo = _mapper.Map<Dispositivo>(request.Dispositivo);
            dispositivo.DispositivoId = Guid.NewGuid();
            dispositivo.FechaCreacion = DateTime.UtcNow;

            // Crear en ThingsBoard antes de guardar en DB para evitar inconsistencias
            try
            {
                await _tbDeviceService.CreateOrUpdateDeviceAsync(
                    null,
                    dispositivo.DispositivoId.ToString(),
                    tipo.Nombre,
                    dispositivo.NumeroSerieDispositivo,
                    null,
                    cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                return Result<Guid>.Failure($"Falló la creación en ThingsBoard: {ex.Message}");
            }

            await _unitOfWork.Dispositivos.AddAsync(dispositivo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(dispositivo.DispositivoId);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            if (inner.Contains("UQ_Dispositivos_NumeroSerie"))
                return Result<Guid>.Failure($"Ya existe un dispositivo con el número de serie '{request.Dispositivo.NumeroSerieDispositivo}'.");
            if (inner.Contains("UQ_Dispositivos_MAC"))
                return Result<Guid>.Failure($"Ya existe un dispositivo con la dirección MAC '{request.Dispositivo.DireccionMac}'.");
            if (inner.Contains("UQ_Dispositivos_IP"))
                return Result<Guid>.Failure($"Ya existe un dispositivo con la dirección IP '{request.Dispositivo.Ipdispositivo}'.");
            if (inner.Contains("FK_Dispositivos_TiposDispositivo"))
                return Result<Guid>.Failure($"El tipo de dispositivo con ID {request.Dispositivo.TipoDispositivoId} no existe.");
            if (inner.Contains("FK_Dispositivos_EstadosDispositivo"))
                return Result<Guid>.Failure($"El estado de dispositivo con ID {request.Dispositivo.EstadoDispositivoId} no existe.");
            if (inner.Contains("FK_Dispositivos_Hoteles"))
                return Result<Guid>.Failure($"El hotel con ID {request.Dispositivo.HotelId} no existe.");
            if (inner.Contains("CHK_Dispositivos_Bateria"))
                return Result<Guid>.Failure("El nivel de batería debe estar entre 0 y 100.");
            return Result<Guid>.Failure($"Error de base de datos: {inner}");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Error inesperado al crear el dispositivo: {ex.Message}");
        }
    }
}
