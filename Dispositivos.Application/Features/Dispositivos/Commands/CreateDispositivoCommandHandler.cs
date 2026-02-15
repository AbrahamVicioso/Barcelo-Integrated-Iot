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
            var dispositivo = _mapper.Map<Dispositivo>(request.Dispositivo);
            dispositivo.FechaCreacion = DateTime.UtcNow;

            // Create device in local database first
            await _unitOfWork.Dispositivos.AddAsync(dispositivo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // If Thingsboard integration is enabled, create device in Thingsboard
            if (!string.IsNullOrEmpty(request.Dispositivo.NumeroSerieDispositivo))
            {
                try
                {
                    var thingsboardResponse = await _tbDeviceService.CreateOrUpdateDeviceAsync(
                        dispositivo.DispositivoId.ToString(),
                        dispositivo.TipoDispositivo,
                        dispositivo.NumeroSerieDispositivo,
                        dispositivo.DispositivoId.ToString(),
                        cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (HttpRequestException ex)
                {
                    return Result<Guid>.Failure(
                        $"Dispositivo creado localmente pero falló la creación en Thingsboard: {ex.Message}");
                }
            }

            return Result<Guid>.Success(dispositivo.DispositivoId);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Error al crear el dispositivo: {ex.Message}");
        }
    }
}
