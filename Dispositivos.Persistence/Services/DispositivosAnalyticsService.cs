using Dispositivos.Application.DTOs.Analytics;
using Dispositivos.Application.Interfaces;
using Dispositivos.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Dispositivos.Persistence.Services;

public class DispositivosAnalyticsService : IDispositivosAnalyticsService
{
    private readonly BarceloIoTDatabaseContext _context;

    public DispositivosAnalyticsService(BarceloIoTDatabaseContext context)
    {
        _context = context;
    }

    public async Task<AccesosResumenDto> GetAccesosResumenAsync(CancellationToken ct = default)
    {
        var utcNow = DateTime.UtcNow;
        var hoyUtc4 = utcNow.AddHours(-4);
        var inicioHoy = new DateTime(hoyUtc4.Year, hoyUtc4.Month, hoyUtc4.Day, 0, 0, 0, DateTimeKind.Unspecified).AddHours(4);
        var finHoy = inicioHoy.AddDays(1);

        var diasDesdelunes = ((int)hoyUtc4.DayOfWeek - 1 + 7) % 7;
        var inicioSemana = new DateTime(hoyUtc4.Year, hoyUtc4.Month, hoyUtc4.Day, 0, 0, 0, DateTimeKind.Unspecified)
            .AddDays(-diasDesdelunes).AddHours(4);

        var inicioMes = new DateTime(hoyUtc4.Year, hoyUtc4.Month, 1, 0, 0, 0, DateTimeKind.Unspecified).AddHours(4);

        var hoy = await _context.RegistrosAccesos
            .CountAsync(r => r.FechaHoraAcceso >= inicioHoy && r.FechaHoraAcceso < finHoy, ct);

        var semana = await _context.RegistrosAccesos
            .CountAsync(r => r.FechaHoraAcceso >= inicioSemana && r.FechaHoraAcceso < finHoy, ct);

        var mes = await _context.RegistrosAccesos
            .CountAsync(r => r.FechaHoraAcceso >= inicioMes && r.FechaHoraAcceso < finHoy, ct);

        return new AccesosResumenDto { Hoy = hoy, Semana = semana, Mes = mes };
    }

    public async Task<DispositivosResumenDto> GetDispositivosResumenAsync(CancellationToken ct = default)
    {
        var dispositivos = await _context.Dispositivos
            .Include(d => d.EstadoDispositivo)
            .ToListAsync(ct);

        var total = dispositivos.Count;
        const int umbralBateria = 20;

        var porEstado = dispositivos
            .GroupBy(d => (d.EstadoDispositivo?.Descripcion ?? "").ToLower())
            .ToDictionary(g => g.Key, g => g.Count());

        var operativos = porEstado.GetValueOrDefault("operativo") + porEstado.GetValueOrDefault("operativos");
        var mantenimiento = porEstado.GetValueOrDefault("mantenimiento");
        var falla = porEstado.GetValueOrDefault("falla") + porEstado.GetValueOrDefault("avería") + porEstado.GetValueOrDefault("averia");
        var inactivos = porEstado.GetValueOrDefault("inactivo") + porEstado.GetValueOrDefault("inactivos");

        if (operativos == 0 && mantenimiento == 0 && falla == 0 && inactivos == 0)
        {
            operativos = dispositivos.Count(d => d.EstaEnLinea);
            inactivos = dispositivos.Count(d => !d.EstaEnLinea);
        }

        var saludGeneral = total > 0 ? Math.Round((decimal)operativos / total * 100, 1) : 0m;

        var firmwareDesactualizado = dispositivos
            .GroupBy(d => d.TipoDispositivoId)
            .Sum(g =>
            {
                var moda = g
                    .GroupBy(d => d.VersionFirmware)
                    .OrderByDescending(fg => fg.Count())
                    .First().Key;
                return g.Count(d => d.VersionFirmware != moda);
            });

        return new DispositivosResumenDto
        {
            Total = total,
            Operativos = operativos,
            Mantenimiento = mantenimiento,
            Falla = falla,
            Inactivos = inactivos,
            SaludGeneral = saludGeneral,
            BateriaBaja = dispositivos.Count(d => d.NivelBateria < umbralBateria),
            Offline = dispositivos.Count(d => !d.EstaEnLinea),
            FirmwareDesactualizado = firmwareDesactualizado
        };
    }

    public async Task<List<AccesoPorTipoDto>> GetAccesosPorTipoAsync(CancellationToken ct = default)
    {
        var utcNow = DateTime.UtcNow;
        var hoyUtc4 = utcNow.AddHours(-4);
        var inicioMes = new DateTime(hoyUtc4.Year, hoyUtc4.Month, 1, 0, 0, 0, DateTimeKind.Unspecified).AddHours(4);

        var grupos = await _context.RegistrosAccesos
            .Where(r => r.FechaHoraAcceso >= inicioMes)
            .GroupBy(r => r.TipoAcceso)
            .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
            .OrderByDescending(g => g.Cantidad)
            .ToListAsync(ct);

        var total = grupos.Sum(g => g.Cantidad);
        if (total == 0) return new List<AccesoPorTipoDto>();

        return grupos.Select(g => new AccesoPorTipoDto
        {
            Tipo = g.Tipo ?? "Sin tipo",
            Cantidad = g.Cantidad,
            Porcentaje = Math.Round((decimal)g.Cantidad / total * 100, 1)
        }).ToList();
    }

    public async Task<IncidentesResumenDto> GetIncidentesResumenAsync(CancellationToken ct = default)
    {
        var severidadesAltas = new[] { "Critico", "Error" };
        var severidadesBajas = new[] { "Advertencia", "Informativo" };

        var activos = await _context.EventosSistema
            .CountAsync(e => !e.EstaResuelto && severidadesAltas.Contains(e.Severidad), ct);

        var pendientes = await _context.EventosSistema
            .CountAsync(e => !e.EstaResuelto && severidadesBajas.Contains(e.Severidad), ct);

        var resueltos = await _context.EventosSistema
            .CountAsync(e => e.EstaResuelto, ct);

        var porSeveridad = await _context.EventosSistema
            .Where(e => !e.EstaResuelto)
            .GroupBy(e => e.Severidad)
            .Select(g => new IncidentePorSeveridadDto { Severidad = g.Key, Cantidad = g.Count() })
            .ToListAsync(ct);

        return new IncidentesResumenDto
        {
            Activos = activos,
            Pendientes = pendientes,
            Resueltos = resueltos,
            PorSeveridad = porSeveridad
        };
    }
}
