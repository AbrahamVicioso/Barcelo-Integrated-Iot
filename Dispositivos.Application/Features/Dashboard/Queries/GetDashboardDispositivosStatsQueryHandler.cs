using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.Dashboard.Queries;

public class GetDashboardDispositivosStatsQueryHandler
    : IRequestHandler<GetDashboardDispositivosStatsQuery, Result<DashboardDispositivosStatsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardDispositivosStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DashboardDispositivosStatsDto>> Handle(
        GetDashboardDispositivosStatsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var stats = new DashboardDispositivosStatsDto
            {
                Credenciales = await GetCredencialesStats(request),
                Dispositivos = await GetDispositivosStats(request),
                Cerraduras = await GetCerradurasStats(request),
                Auditoria = await GetAuditoriaStats(request)
            };

            return Result<DashboardDispositivosStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            return Result<DashboardDispositivosStatsDto>.Failure(
                $"Error al obtener estadísticas del dashboard: {ex.Message}");
        }
    }

    private async Task<CredencialesStatsDto> GetCredencialesStats(GetDashboardDispositivosStatsQuery request)
    {
        var credenciales = await _unitOfWork.CredencialesAcceso.GetAll();
        var ahora = DateTime.UtcNow;
        var diasParaVencer = 7; // Credenciales que vencen en los próximos 7 días

        return new CredencialesStatsDto
        {
            Total = credenciales.Count(),
            Activas = credenciales.Count(c => c.EstaActiva && c.FechaExpiracion > ahora),
            PorVencer = credenciales.Count(c =>
                c.EstaActiva &&
                c.FechaExpiracion > ahora &&
                c.FechaExpiracion <= ahora.AddDays(diasParaVencer)),
            Expiradas = credenciales.Count(c => c.FechaExpiracion <= ahora || !c.EstaActiva),
            PorTipo = credenciales
                .GroupBy(c => c.TipoCredencial ?? "Sin tipo")
                .Select(g => new CredencialesPorTipoDto
                {
                    Tipo = g.Key,
                    Total = g.Count(),
                    Activas = g.Count(c => c.EstaActiva && c.FechaExpiracion > ahora)
                })
                .ToList(),
            ProximasVencer = credenciales
                .Where(c => c.EstaActiva && c.FechaExpiracion > ahora && c.FechaExpiracion <= ahora.AddDays(diasParaVencer))
                .OrderBy(c => c.FechaExpiracion)
                .Take(10)
                .Select(c => new CredencialProximaVencerDto
                {
                    CredencialId = c.CredencialId,
                    Tipo = c.TipoCredencial ?? "Sin tipo",
                    CodigoPin = $"****{c.CodigoPin?.Substring(Math.Max(0, (c.CodigoPin?.Length ?? 4) - 4))}",
                    DiasRestantes = (int)(c.FechaExpiracion - ahora).TotalDays,
                    FechaExpiracion = c.FechaExpiracion
                })
                .ToList()
        };
    }

    private async Task<DispositivosStatsDto> GetDispositivosStats(GetDashboardDispositivosStatsQuery request)
    {
        var dispositivos = await _unitOfWork.Dispositivos.GetAll();

        // Filtrar por hotel si se especifica
        if (request.HotelId.HasValue)
        {
            dispositivos = dispositivos.Where(d => d.HotelId == request.HotelId.Value);
        }

        var dispositivosList = dispositivos.ToList();
        var umbralBateriaBaja = 20; // Batería menor al 20%

        var operativos = dispositivosList.Count(d => d.EstaEnLinea);
        var total = dispositivosList.Count;
        var saludPorcentaje = total > 0 ? (decimal)operativos / total * 100 : 0;

        return new DispositivosStatsDto
        {
            Total = total,
            Operativos = operativos,
            BateriaBaja = dispositivosList.Count(d => d.NivelBateria < umbralBateriaBaja),
            Offline = dispositivosList.Count(d => !d.EstaEnLinea),
            SaludPorcentaje = Math.Round(saludPorcentaje, 1),
            PorTipo = dispositivosList
                .GroupBy(d => d.TipoDispositivo?.Nombre ?? "Sin tipo")
                .Select(g => new DispositivosPorTipoDto
                {
                    Tipo = g.Key,
                    Total = g.Count(),
                    Online = g.Count(d => d.EstaEnLinea)
                })
                .ToList(),
            AlertasBateria = dispositivosList
                .Where(d => d.NivelBateria < umbralBateriaBaja)
                .OrderBy(d => d.NivelBateria)
                .Take(10)
                .Select(d => new DispositivoAlertaBateriaDto
                {
                    DispositivoId = d.DispositivoId,
                    NumeroSerie = d.NumeroSerieDispositivo ?? "Sin serie",
                    NivelBateria = d.NivelBateria,
                    HotelId = d.HotelId,
                    HabitacionId = d.CerradurasInteligentes?.FirstOrDefault()?.HabitacionId
                })
                .ToList()
        };
    }

    private async Task<CerradurasStatsDto> GetCerradurasStats(GetDashboardDispositivosStatsQuery request)
    {
        var cerraduras = await _unitOfWork.CerradurasInteligente.GetAll();
        var cerradurasList = cerraduras.ToList();

        // Filtrar por hotel si se especifica (a través del dispositivo)
        if (request.HotelId.HasValue)
        {
            cerradurasList = cerradurasList
                .Where(c => c.Dispositivo?.HotelId == request.HotelId.Value)
                .ToList();
        }

        var total = cerradurasList.Count;
        var online = cerradurasList.Count(c => c.Dispositivo?.EstaEnLinea == true);

        return new CerradurasStatsDto
        {
            Total = total,
            Online = online,
            Offline = total - online,
            Abiertas = cerradurasList.Count(c => c.EstadoPuerta?.ToLower() == "abierta"),
            Cerradas = cerradurasList.Count(c => c.EstadoPuerta?.ToLower() == "cerrada"),
            SoportanOffline = cerradurasList.Count(c => c.SoportaModoOffline),
            PromedioAperturasHoy = total > 0
                ? Math.Round((decimal)cerradurasList.Sum(c => c.ContadorAperturas) / total, 1)
                : 0
        };
    }

    private async Task<AuditoriaStatsDto> GetAuditoriaStats(GetDashboardDispositivosStatsQuery request)
    {
        var registros = await _unitOfWork.RegistrosAuditorium.GetAllAsync();
        var ahora = DateTime.UtcNow;

        // Filtrar por hotel si se especifica
        if (request.HotelId.HasValue)
        {
            registros = registros.Where(r => r.HotelId == request.HotelId.Value);
        }

        // Filtrar por rango de fechas si se especifica
        if (request.Desde.HasValue)
        {
            registros = registros.Where(r => r.FechaHora >= request.Desde.Value);
        }
        if (request.Hasta.HasValue)
        {
            registros = registros.Where(r => r.FechaHora <= request.Hasta.Value);
        }

        var registrosList = registros.ToList();

        return new AuditoriaStatsDto
        {
            Ultimas24h = registrosList.Count(r => r.FechaHora >= ahora.AddDays(-1)),
            UltimaSemana = registrosList.Count(r => r.FechaHora >= ahora.AddDays(-7)),
            RegistrosRecientes = registrosList
                .OrderByDescending(r => r.FechaHora)
                .Take(10)
                .Select(r => new AuditoriaRecienteDto
                {
                    AuditoriaId = r.AuditoriaId,
                    Accion = r.Accion ?? "Sin acción",
                    TipoEntidad = r.TipoEntidad ?? "Sin tipo",
                    Resultado = r.Resultado ?? "Sin resultado",
                    UsuarioId = r.UsuarioId ?? "Sin usuario",
                    FechaHora = r.FechaHora
                })
                .ToList()
        };
    }
}
