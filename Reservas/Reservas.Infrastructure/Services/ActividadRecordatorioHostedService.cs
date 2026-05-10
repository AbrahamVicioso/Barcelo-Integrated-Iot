using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;
using Reservas.Application.Interfaces;

namespace Reservas.Infrastructure.Services;

public class ActividadRecordatorioHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IReservaActividadKafkaProducer _kafkaProducer;
    private readonly ILogger<ActividadRecordatorioHostedService> _logger;
    private readonly int _minutosAntes;
    private readonly TimeSpan _intervalo;

    public ActividadRecordatorioHostedService(
        IServiceScopeFactory scopeFactory,
        IReservaActividadKafkaProducer kafkaProducer,
        IConfiguration configuration,
        ILogger<ActividadRecordatorioHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
        _minutosAntes = configuration.GetValue<int>("ActividadRecordatorio:MinutosAntes", 30);
        _intervalo = TimeSpan.FromMinutes(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ActividadRecordatorioHostedService iniciado. Revisando cada {Intervalo}min, notificando {MinutosAntes}min antes.",
            _intervalo.TotalMinutes, _minutosAntes);

        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarRecordatoriosAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando recordatorios de actividades");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }

    private async Task ProcesarRecordatoriosAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var usuariosService = scope.ServiceProvider.GetRequiredService<IUsuariosApiService>();

        var proximas = (await unitOfWork.ReservasActividades.GetProximasParaRecordatorioAsync(_minutosAntes, cancellationToken)).ToList();

        if (proximas.Count == 0) return;

        _logger.LogInformation("Encontradas {Count} actividades próximas para recordatorio", proximas.Count);

        foreach (var reserva in proximas)
        {
            try
            {
                var huesped = await usuariosService.GetHuespedByIdAsync(reserva.HuespedId, cancellationToken);

                if (huesped == null || string.IsNullOrEmpty(huesped.CorreoElectronico))
                {
                    _logger.LogWarning(
                        "Huésped {HuespedId} sin email para recordatorio de actividad {ReservaActividadId}",
                        reserva.HuespedId, reserva.ReservaActividadId);
                }
                else
                {
                    var evt = new ActividadRecordatorioEvent
                    {
                        ReservaActividadId = reserva.ReservaActividadId,
                        ActividadId = reserva.ActividadId,
                        NombreActividad = reserva.Actividad?.NombreActividad ?? string.Empty,
                        Descripcion = reserva.Actividad?.Descripcion ?? string.Empty,
                        Ubicacion = reserva.Actividad?.Ubicacion ?? string.Empty,
                        FechaReserva = DateTime.SpecifyKind(reserva.FechaReserva, DateTimeKind.Unspecified),
                        HoraReserva = reserva.HoraReserva,
                        HuespedId = reserva.HuespedId,
                        Email = huesped.CorreoElectronico,
                        NombreCompleto = huesped.NombreCompleto ?? string.Empty,
                        NumeroPersonas = reserva.NumeroPersonas,
                        MinutosAntes = _minutosAntes
                    };

                    await _kafkaProducer.PublishActividadRecordatorioAsync(evt, cancellationToken);
                }

                // Marcar como enviado independientemente del email para no reintentar
                reserva.RecordatorioEnviado = true;
                reserva.FechaRecordatorio = DateTime.Now;
                await unitOfWork.ReservasActividades.UpdateAsync(reserva, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error enviando recordatorio para reservaActividad {ReservaActividadId}", reserva.ReservaActividadId);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
