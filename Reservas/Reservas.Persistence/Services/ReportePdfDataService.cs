using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Reservas.Application.DTOs.Reports;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;
using Reservas.Persistence.Data;

namespace Reservas.Persistence.Services;

public class ReportePdfDataService : IReportePdfDataService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHuespedRepository _huespedes;
    private readonly BarceloReservasContext _context;
    private const int EstadoCancelada = 4;

    public ReportePdfDataService(IUnitOfWork unitOfWork, IHuespedRepository huespedes, BarceloReservasContext context)
    {
        _unitOfWork = unitOfWork;
        _huespedes = huespedes;
        _context = context;
    }

    public async Task<List<ReporteReservaItemDto>> GetReservasPeriodoAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct = default)
    {
        var reservas = (await GetReservasAsync(fechaInicio, fechaFin, ct))
            .Where(r => r.EstadoReservaId != EstadoCancelada)
            .ToList();

        var hoteles = (await _unitOfWork.Hoteles.GetAll()).ToList();
        var habitaciones = (await _unitOfWork.Habitaciones.GetAll()).ToList();
        var estados = (await _unitOfWork.EstadosReserva.GetAllAsync(ct)).ToList();
        var nombres = await BuildNombresAsync(reservas, ct);

        return reservas
            .Select(r =>
            {
                var habitacion = habitaciones.FirstOrDefault(h => h.HabitacionId == r.HabitacionId);
                var hotel = habitacion is not null ? hoteles.FirstOrDefault(h => h.HotelId == habitacion.HotelId) : null;
                var estado = estados.FirstOrDefault(e => e.EstadoReservaId == r.EstadoReservaId);

                return new ReporteReservaItemDto(
                    r.NumeroReserva ?? r.ReservaId.ToString(),
                    nombres.GetValueOrDefault(r.HuespedId, $"Huésped #{r.HuespedId}"),
                    r.FechaCheckIn,
                    r.FechaCheckOut,
                    r.MontoTotal,
                    r.MontoPagado,
                    estado?.Nombre ?? "Desconocido"
                );
            })
            .OrderBy(r => r.FechaCheckIn)
            .ToList();
    }

    public async Task<List<ReportePuntualidadItemDto>> GetCheckInTempranoAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct = default)
    {
        var (reservas, habitaciones, hoteles) = await GetBaseDataAsync(fechaInicio, fechaFin, ct);
        var filtered = reservas.Where(r => r.CheckInRealizado.HasValue && r.CheckInRealizado.Value < r.FechaCheckIn).ToList();
        var nombres = await BuildNombresAsync(filtered, ct);

        return filtered
            .Select(r => MapPuntualidad(r, r.FechaCheckIn, r.CheckInRealizado!.Value, habitaciones, hoteles, nombres))
            .OrderByDescending(r => r.Diferencia)
            .ToList();
    }

    public async Task<List<ReportePuntualidadItemDto>> GetCheckInTardeAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct = default)
    {
        var (reservas, habitaciones, hoteles) = await GetBaseDataAsync(fechaInicio, fechaFin, ct);
        var filtered = reservas.Where(r => r.CheckInRealizado.HasValue && r.CheckInRealizado.Value > r.FechaCheckIn).ToList();
        var nombres = await BuildNombresAsync(filtered, ct);

        return filtered
            .Select(r => MapPuntualidad(r, r.FechaCheckIn, r.CheckInRealizado!.Value, habitaciones, hoteles, nombres))
            .OrderByDescending(r => r.Diferencia)
            .ToList();
    }

    public async Task<List<ReportePuntualidadItemDto>> GetCheckOutTempranoAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct = default)
    {
        var (reservas, habitaciones, hoteles) = await GetBaseDataAsync(fechaInicio, fechaFin, ct, porCheckOut: true);
        var filtered = reservas.Where(r => r.CheckOutRealizado.HasValue && r.CheckOutRealizado.Value < r.FechaCheckOut).ToList();
        var nombres = await BuildNombresAsync(filtered, ct);

        return filtered
            .Select(r => MapPuntualidad(r, r.FechaCheckOut, r.CheckOutRealizado!.Value, habitaciones, hoteles, nombres))
            .OrderByDescending(r => r.Diferencia)
            .ToList();
    }

    public async Task<List<ReportePuntualidadItemDto>> GetCheckOutTardeAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct = default)
    {
        var (reservas, habitaciones, hoteles) = await GetBaseDataAsync(fechaInicio, fechaFin, ct, porCheckOut: true);
        var filtered = reservas.Where(r => r.CheckOutRealizado.HasValue && r.CheckOutRealizado.Value > r.FechaCheckOut).ToList();
        var nombres = await BuildNombresAsync(filtered, ct);

        return filtered
            .Select(r => MapPuntualidad(r, r.FechaCheckOut, r.CheckOutRealizado!.Value, habitaciones, hoteles, nombres))
            .OrderByDescending(r => r.Diferencia)
            .ToList();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<IEnumerable<Reservas.Domain.Entites.Reserva>> GetReservasAsync(
        DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct)
    {
        if (fechaInicio.HasValue && fechaFin.HasValue)
            return await _unitOfWork.Reservas.GetReservasByFechaRangoAsync(fechaInicio.Value, fechaFin.Value, ct);

        return await _unitOfWork.Reservas.GetAllAsync(ct);
    }

    private async Task<Dictionary<int, string>> BuildNombresAsync(
        IEnumerable<Reservas.Domain.Entites.Reserva> reservas, CancellationToken ct)
    {
        var dict = new Dictionary<int, string>();
        foreach (var huespedId in reservas.Select(r => r.HuespedId).Distinct())
        {
            var info = await _huespedes.GetHuespedEmailYNombreAsync(huespedId, ct);
            dict[huespedId] = info?.NombreCompleto ?? $"Huésped #{huespedId}";
        }
        return dict;
    }

    private async Task<(List<Reservas.Domain.Entites.Reserva> reservas,
                        List<Reservas.Domain.Entities.Habitacion> habitaciones,
                        List<Reservas.Domain.Entities.Hotel> hoteles)>
        GetBaseDataAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct, bool porCheckOut = false)
    {
        var reservas = await GetReservasAsync(fechaInicio, fechaFin, ct);

        if (porCheckOut && fechaInicio.HasValue && fechaFin.HasValue)
            reservas = reservas.Where(r => r.FechaCheckOut >= fechaInicio.Value && r.FechaCheckOut <= fechaFin.Value);

        var habitaciones = (await _unitOfWork.Habitaciones.GetAll()).ToList();
        var hoteles = (await _unitOfWork.Hoteles.GetAll()).ToList();
        return (reservas.ToList(), habitaciones, hoteles);
    }

    // ── Nuevos reportes ───────────────────────────────────────────────────────

    public async Task<List<ReporteHabitacionItemDto>> GetHabitacionesAsync(int? tipoId, int? estadoId, int? hotelId, CancellationToken ct = default)
    {
        var habitaciones = (await _unitOfWork.Habitaciones.GetAll()).ToList();

        if (tipoId.HasValue)   habitaciones = habitaciones.Where(h => h.TipoHabitacionId == tipoId.Value).ToList();
        if (estadoId.HasValue) habitaciones = habitaciones.Where(h => h.EstadoHabitacionId == estadoId.Value).ToList();
        if (hotelId.HasValue)  habitaciones = habitaciones.Where(h => h.HotelId == hotelId.Value).ToList();

        return habitaciones
            .OrderBy(h => h.Hotel?.Nombre)
            .ThenBy(h => h.NumeroHabitacion)
            .Select(h => new ReporteHabitacionItemDto(
                h.Hotel?.Nombre ?? "?",
                h.NumeroHabitacion,
                h.TipoHabitacion?.Nombre ?? "?",
                h.EstadoHabitacion?.Descripcion ?? "?",
                h.Piso,
                h.CapacidadMaxima,
                h.PrecioPorNoche))
            .ToList();
    }

    public async Task<List<ReporteActividadResumenItemDto>> GetActividadesResumenAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct = default)
    {
        var todasReservas = (await _unitOfWork.ReservasActividades.GetAllAsync(ct)).ToList();
        var actividades = (await _unitOfWork.ActividadesRecreativas.GetAllAsync(ct)).ToList();
        var hoteles = (await _unitOfWork.Hoteles.GetAll()).ToList();

        if (fechaInicio.HasValue) todasReservas = todasReservas.Where(r => r.FechaReserva >= fechaInicio.Value).ToList();
        if (fechaFin.HasValue)   todasReservas = todasReservas.Where(r => r.FechaReserva <= fechaFin.Value).ToList();

        return actividades
            .Select(a =>
            {
                var reservas = todasReservas.Where(r => r.ActividadId == a.ActividadId).ToList();
                var hotel = hoteles.FirstOrDefault(h => h.HotelId == a.HotelId);
                return new ReporteActividadResumenItemDto(
                    a.NombreActividad,
                    a.Categoria ?? "-",
                    hotel?.Nombre ?? "?",
                    reservas.Count,
                    reservas.Sum(r => r.NumeroPersonas),
                    reservas.Sum(r => r.MontoTotal),
                    a.EstaActiva);
            })
            .OrderByDescending(r => r.TotalReservas)
            .ToList();
    }

    public async Task<List<ReporteActividadSinAccesoItemDto>> GetActividadesSinAccesoAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct = default)
    {
        var todasReservas = (await _unitOfWork.ReservasActividades.GetAllAsync(ct)).ToList();
        var actividades = (await _unitOfWork.ActividadesRecreativas.GetAllAsync(ct))
            .ToDictionary(a => a.ActividadId, a => a.NombreActividad);

        var sinAcceso = todasReservas
            .Where(r => r.FechaReserva.Date < DateTime.Now.Date)
            .Where(r => r.EstadoReservaActividadId == EstadoReservaActividad.Pendiente)
            .Where(r => fechaInicio == null || r.FechaReserva >= fechaInicio.Value)
            .Where(r => fechaFin == null || r.FechaReserva <= fechaFin.Value)
            .ToList();

        var nombres = await BuildNombresByIdsAsync(sinAcceso.Select(r => r.HuespedId), ct);

        return sinAcceso
            .OrderByDescending(r => r.FechaReserva)
            .Select(r => new ReporteActividadSinAccesoItemDto(
                actividades.GetValueOrDefault(r.ActividadId, "?"),
                nombres.GetValueOrDefault(r.HuespedId, $"Huésped #{r.HuespedId}"),
                r.FechaReserva,
                r.HoraReserva,
                r.NumeroPersonas,
                r.Estado ?? "Pendiente",
                r.MontoTotal))
            .ToList();
    }

    public async Task<List<ReporteHuespedItemDto>> GetHuespedesAsync(DateTime? fechaInicio, DateTime? fechaFin, bool? soloVip, CancellationToken ct = default)
    {
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        var sql = new StringBuilder(@"
            SELECT h.NombreCompleto, td.Nombre AS TipoDocumento, h.NumeroDocumento,
                   h.Nacionalidad, h.EsVIP, u.Email, h.FechaRegistro
            FROM Huespedes h
            LEFT JOIN Users u ON h.UsuarioId = u.Id
            LEFT JOIN TiposDocumento td ON h.TipoDocumentoId = td.TipoDocumentoId
            WHERE 1=1");

        var parametros = new List<SqlParameter>();
        if (fechaInicio.HasValue) { sql.Append(" AND h.FechaRegistro >= @fi"); parametros.Add(new SqlParameter("@fi", fechaInicio.Value)); }
        if (fechaFin.HasValue)   { sql.Append(" AND h.FechaRegistro <= @ff"); parametros.Add(new SqlParameter("@ff", fechaFin.Value)); }
        if (soloVip.HasValue)    { sql.Append(" AND h.EsVIP = @vip");         parametros.Add(new SqlParameter("@vip", soloVip.Value)); }
        sql.Append(" ORDER BY h.FechaRegistro DESC");

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.ToString();
        foreach (var p in parametros) cmd.Parameters.Add(p);

        var result = new List<ReporteHuespedItemDto>();
        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new ReporteHuespedItemDto(
                reader.GetString(0),
                reader.IsDBNull(1) ? "-" : reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? "-" : reader.GetString(3),
                reader.GetBoolean(4),
                reader.IsDBNull(5) ? "-" : reader.GetString(5),
                reader.GetDateTime(6)));
        }
        return result;
    }

    public async Task<List<ReportePersonalItemDto>> GetPersonalAsync(int? departamentoId, bool? soloActivos, CancellationToken ct = default)
    {
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        var sql = new StringBuilder(@"
            SELECT p.NombreCompleto, p.NumeroEmpleado, p.FechaContratacion, p.EstaActivo,
                   ISNULL(p.Turno, '') AS Turno,
                   ISNULL(pu.Nombre, '-') AS Puesto, ISNULL(d.Nombre, '-') AS Departamento, ISNULL(h.Nombre, '-') AS Hotel
            FROM Personal p
            LEFT JOIN Puestos pu ON p.PuestoId = pu.PuestoId
            LEFT JOIN Departamentos d ON p.DepartamentoId = d.DepartamentoId
            LEFT JOIN Hoteles h ON p.HotelId = h.HotelId
            WHERE 1=1");

        var parametros = new List<SqlParameter>();
        if (departamentoId.HasValue) { sql.Append(" AND p.DepartamentoId = @depId"); parametros.Add(new SqlParameter("@depId", departamentoId.Value)); }
        if (soloActivos.HasValue)    { sql.Append(" AND p.EstaActivo = @activo");    parametros.Add(new SqlParameter("@activo", soloActivos.Value)); }
        sql.Append(" ORDER BY d.Nombre, p.NombreCompleto");

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.ToString();
        foreach (var p in parametros) cmd.Parameters.Add(p);

        var result = new List<ReportePersonalItemDto>();
        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new ReportePersonalItemDto(
                NombreCompleto:    reader.GetString(0),
                NumeroEmpleado:    reader.GetString(1),
                Puesto:            reader.GetString(5),
                Departamento:      reader.GetString(6),
                Hotel:             reader.GetString(7),
                Turno:             reader.GetString(4),
                FechaContratacion: reader.GetDateTime(2),
                EstaActivo:        reader.GetBoolean(3)));
        }
        return result;
    }

    private async Task<Dictionary<int, string>> BuildNombresByIdsAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        var dict = new Dictionary<int, string>();
        foreach (var huespedId in ids.Distinct())
        {
            var info = await _huespedes.GetHuespedEmailYNombreAsync(huespedId, ct);
            dict[huespedId] = info?.NombreCompleto ?? $"Huésped #{huespedId}";
        }
        return dict;
    }

    private static ReportePuntualidadItemDto MapPuntualidad(
        Reservas.Domain.Entites.Reserva r,
        DateTime programada,
        DateTime realizada,
        List<Reservas.Domain.Entities.Habitacion> habitaciones,
        List<Reservas.Domain.Entities.Hotel> hoteles,
        Dictionary<int, string> nombres)
    {
        var habitacion = habitaciones.FirstOrDefault(h => h.HabitacionId == r.HabitacionId);
        var hotel = habitacion is not null ? hoteles.FirstOrDefault(h => h.HotelId == habitacion.HotelId) : null;
        var habitacionDesc = habitacion is not null
            ? $"{hotel?.Nombre ?? "?"} - Hab. {habitacion.NumeroHabitacion}"
            : "Sin asignar";

        return new ReportePuntualidadItemDto(
            r.NumeroReserva ?? r.ReservaId.ToString(),
            nombres.GetValueOrDefault(r.HuespedId, $"Huésped #{r.HuespedId}"),
            habitacionDesc,
            programada,
            realizada,
            (realizada - programada).Duration()
        );
    }
}
