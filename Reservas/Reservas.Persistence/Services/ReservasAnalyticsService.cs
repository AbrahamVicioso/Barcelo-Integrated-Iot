using Reservas.Application.DTOs.Analytics;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entities;

namespace Reservas.Persistence.Services;

public class ReservasAnalyticsService : IReservasAnalyticsService
{
    private readonly IUnitOfWork _unitOfWork;

    private static readonly string[] MesesEspanol =
    {
        "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
        "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
    };

    private const decimal ObjetivoOcupacion = 85.0m;
    private const int EstadoCancelada = 4;

    public ReservasAnalyticsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OcupacionResumenDto> GetOcupacionResumenAsync(CancellationToken ct = default)
    {
        var habitaciones = (await _unitOfWork.Habitaciones.GetAll()).ToList();
        var totalHabitaciones = habitaciones.Count;

        if (totalHabitaciones == 0)
            return new OcupacionResumenDto { Actual = 0, Objetivo = ObjetivoOcupacion, Tendencia = 0 };

        var reservas = (await _unitOfWork.Reservas.GetAllAsync(ct)).ToList();
        var ahora = DateTime.Now;

        // Current occupancy: rooms with active reservation overlapping now
        var reservasActuales = reservas
            .Where(r => r.EstadoReservaId != EstadoCancelada
                        && r.FechaCheckIn <= ahora
                        && r.FechaCheckOut > ahora
                        && r.HabitacionId.HasValue)
            .Select(r => r.HabitacionId!.Value)
            .Distinct()
            .Count();

        var actual = Math.Round((decimal)reservasActuales / totalHabitaciones * 100, 1);

        // Trend: occupancy 7 days ago
        var hace7Dias = ahora.AddDays(-7);
        var ocupadasHace7 = reservas
            .Where(r => r.EstadoReservaId != EstadoCancelada
                        && r.FechaCheckIn <= hace7Dias
                        && r.FechaCheckOut > hace7Dias
                        && r.HabitacionId.HasValue)
            .Select(r => r.HabitacionId!.Value)
            .Distinct()
            .Count();

        var actualHace7 = Math.Round((decimal)ocupadasHace7 / totalHabitaciones * 100, 1);
        var tendencia = Math.Round(actual - actualHace7, 1);

        return new OcupacionResumenDto { Actual = actual, Objetivo = ObjetivoOcupacion, Tendencia = tendencia };
    }

    public async Task<List<OcupacionPorHotelDto>> GetOcupacionPorHotelAsync(CancellationToken ct = default)
    {
        var hoteles = (await _unitOfWork.Hoteles.GetAll()).Where(h => h.EstaActivo).ToList();
        var habitaciones = (await _unitOfWork.Habitaciones.GetAll()).ToList();
        var reservas = (await _unitOfWork.Reservas.GetAllAsync(ct)).ToList();
        var ahora = DateTime.Now;

        var reservasActivas = reservas
            .Where(r => r.EstadoReservaId != EstadoCancelada
                        && r.FechaCheckIn <= ahora
                        && r.FechaCheckOut > ahora
                        && r.HabitacionId.HasValue)
            .Select(r => r.HabitacionId!.Value)
            .ToHashSet();

        var result = new List<OcupacionPorHotelDto>();
        foreach (var hotel in hoteles)
        {
            var habsHotel = habitaciones.Where(h => h.HotelId == hotel.HotelId).ToList();
            var total = habsHotel.Count;
            if (total == 0) continue;

            var ocupadas = habsHotel.Count(h => reservasActivas.Contains(h.HabitacionId));
            var pct = Math.Round((decimal)ocupadas / total * 100, 1);

            result.Add(new OcupacionPorHotelDto
            {
                HotelId = hotel.HotelId,
                HotelNombre = hotel.Nombre,
                Ocupacion = pct
            });
        }

        return result;
    }

    public async Task<List<OcupacionHistoricoDto>> GetOcupacionHistoricoAsync(CancellationToken ct = default)
    {
        var anio = DateTime.Now.Year;
        var ahora = DateTime.Now;

        var habitaciones = (await _unitOfWork.Habitaciones.GetAll()).ToList();
        var totalHabitaciones = habitaciones.Count;

        if (totalHabitaciones == 0)
            return Enumerable.Range(1, 12)
                .Select(m => new OcupacionHistoricoDto
                    { Periodo = MesesEspanol[m - 1], Anio = anio, Mes = m, Valor = 0 })
                .ToList();

        var anioStart = new DateTime(anio, 1, 1);
        var anioEnd = new DateTime(anio + 1, 1, 1);

        var reservas = (await _unitOfWork.Reservas.GetAllAsync(ct))
            .Where(r => r.EstadoReservaId != EstadoCancelada
                        && r.FechaCheckIn < anioEnd
                        && r.FechaCheckOut > anioStart)
            .Select(r => new { r.FechaCheckIn, r.FechaCheckOut })
            .ToList();

        var result = new List<OcupacionHistoricoDto>();

        for (int mes = 1; mes <= 12; mes++)
        {
            var startMes = new DateTime(anio, mes, 1);
            var endMes = startMes.AddMonths(1);

            decimal valor = 0;
            if (startMes <= ahora)
            {
                // For current month, only count days up to today
                var effectiveEnd = (anio == ahora.Year && mes == ahora.Month)
                    ? ahora.Date
                    : endMes;
                var effectiveDays = (int)(effectiveEnd - startMes).TotalDays;
                if (effectiveDays <= 0) effectiveDays = 1;

                long occupiedNights = 0;
                foreach (var r in reservas)
                {
                    var overlapStart = r.FechaCheckIn > startMes ? r.FechaCheckIn : startMes;
                    var overlapEnd = r.FechaCheckOut < effectiveEnd ? r.FechaCheckOut : effectiveEnd;
                    var overlap = (int)(overlapEnd - overlapStart).TotalDays;
                    if (overlap > 0) occupiedNights += overlap;
                }

                valor = Math.Round((decimal)occupiedNights / (totalHabitaciones * effectiveDays) * 100, 1);
            }

            result.Add(new OcupacionHistoricoDto
            {
                Periodo = MesesEspanol[mes - 1],
                Anio = anio,
                Mes = mes,
                Valor = valor
            });
        }

        return result;
    }

    public async Task<ActividadesResumenDto> GetActividadesResumenAsync(CancellationToken ct = default)
    {
        var ahora = DateTime.Now;
        var hoyUtc4 = ahora.AddHours(-4);
        var inicioHoy = new DateTime(hoyUtc4.Year, hoyUtc4.Month, hoyUtc4.Day, 0, 0, 0, DateTimeKind.Unspecified).AddHours(4);
        var finHoy = inicioHoy.AddDays(1);

        var diasDesdelunes = ((int)hoyUtc4.DayOfWeek - 1 + 7) % 7;
        var inicioSemana = new DateTime(hoyUtc4.Year, hoyUtc4.Month, hoyUtc4.Day, 0, 0, 0, DateTimeKind.Unspecified)
            .AddDays(-diasDesdelunes).AddHours(4);

        var todas = (await _unitOfWork.ReservasActividades.GetAllAsync(ct)).ToList();

        var hoy = todas.Count(ra =>
            ra.Estado != "Cancelada"
            && ra.FechaReserva >= inicioHoy
            && ra.FechaReserva < finHoy);

        var semana = todas.Count(ra =>
            ra.Estado != "Cancelada"
            && ra.FechaReserva >= inicioSemana
            && ra.FechaReserva < finHoy);

        return new ActividadesResumenDto { ReservadasHoy = hoy, ReservadasSemana = semana };
    }

    public async Task<List<ActividadPopularDto>> GetActividadesMasPopularesAsync(int top, CancellationToken ct = default)
    {
        var reservasActividades = (await _unitOfWork.ReservasActividades.GetAllAsync(ct))
            .Where(ra => ra.Estado != "Cancelada")
            .ToList();

        var actividades = (await _unitOfWork.ActividadesRecreativas.GetAllAsync(ct))
            .ToDictionary(a => a.ActividadId, a => a.NombreActividad ?? "Sin nombre");

        return reservasActividades
            .GroupBy(ra => ra.ActividadId)
            .Select(g => new ActividadPopularDto
            {
                ActividadId = g.Key,
                Actividad = actividades.GetValueOrDefault(g.Key, "Sin nombre"),
                Reservas = g.Count()
            })
            .OrderByDescending(a => a.Reservas)
            .Take(top)
            .ToList();
    }
}
