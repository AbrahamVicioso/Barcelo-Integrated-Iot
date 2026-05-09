using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;

namespace Reservas.Application.Features.Dashboard.Queries;

public class GetDashboardReservasStatsQueryHandler
    : IRequestHandler<GetDashboardReservasStatsQuery, Result<DashboardReservasStatsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardReservasStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DashboardReservasStatsDto>> Handle(
        GetDashboardReservasStatsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var stats = new DashboardReservasStatsDto
            {
                Hoteles = await GetHotelesStats(request, cancellationToken),
                Habitaciones = await GetHabitacionesStats(request, cancellationToken),
                Reservas = await GetReservasStats(request, cancellationToken),
                Actividades = await GetActividadesStats(request, cancellationToken)
            };

            return Result<DashboardReservasStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            return Result<DashboardReservasStatsDto>.Failure(
                $"Error al obtener estadísticas del dashboard: {ex.Message}");
        }
    }

    private async Task<HotelesStatsDto> GetHotelesStats(
        GetDashboardReservasStatsQuery request,
        CancellationToken cancellationToken)
    {
        var hoteles = await _unitOfWork.Hoteles.GetAll();

        // Filtrar por hotel si se especifica
        if (request.HotelId.HasValue)
        {
            hoteles = hoteles.Where(h => h.HotelId == request.HotelId.Value);
        }

        var hotelesList = hoteles.ToList();

        return new HotelesStatsDto
        {
            Total = hotelesList.Count,
            Activos = hotelesList.Count(h => h.EstaActivo),
            Inactivos = hotelesList.Count(h => !h.EstaActivo),
            PorPais = hotelesList
                .GroupBy(h => h.Pais ?? "Sin país")
                .Select(g => new HotelesPorPaisDto
                {
                    Pais = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(p => p.Cantidad)
                .ToList()
        };
    }

    private async Task<HabitacionesStatsDto> GetHabitacionesStats(
        GetDashboardReservasStatsQuery request,
        CancellationToken cancellationToken)
    {
        var habitaciones = await _unitOfWork.Habitaciones.GetAll();
        var reservas = await _unitOfWork.Reservas.GetAllAsync(cancellationToken);

        // Filtrar por hotel si se especifica
        if (request.HotelId.HasValue)
        {
            habitaciones = habitaciones.Where(h => h.HotelId == request.HotelId.Value);
        }

        var habitacionesList = habitaciones.ToList();

        // Obtener IDs de habitaciones ocupadas (con reserva activa)
        var reservasActivas = reservas.Where(r =>
            r.EstadoReservaId != EstadoReserva.Cancelada).ToList();

        var habitacionesOcupadas = reservasActivas
            .Where(r => r.HabitacionId.HasValue)
            .Select(r => r.HabitacionId.Value)
            .Distinct()
            .ToHashSet();

        var totalHabitaciones = habitacionesList.Count;
        // IDs de EstadoHabitacion: 1=Disponible, 2=Ocupada, 3=Mantenimiento, 4=Fuera de Servicio, 5=Limpieza
        var disponibles = habitacionesList.Count(h =>
            h.EstadoHabitacionId == 1 &&
            !habitacionesOcupadas.Contains(h.HabitacionId));
        var ocupadas = habitacionesList.Count(h => habitacionesOcupadas.Contains(h.HabitacionId));
        var mantenimiento = habitacionesList.Count(h => h.EstadoHabitacionId == 3);
        var limpieza = habitacionesList.Count(h => h.EstadoHabitacionId == 5);

        var ocupacionPorcentaje = totalHabitaciones > 0
            ? (decimal)ocupadas / totalHabitaciones * 100
            : 0;

        return new HabitacionesStatsDto
        {
            Total = totalHabitaciones,
            Disponibles = disponibles,
            Ocupadas = ocupadas,
            Mantenimiento = mantenimiento,
            Limpieza = limpieza,
            OcupacionPorcentaje = Math.Round(ocupacionPorcentaje, 1),
            PorTipo = habitacionesList
                .GroupBy(h => h.TipoHabitacion?.Nombre ?? "Sin tipo")
                .Select(g => new HabitacionesPorTipoDto
                {
                    Tipo = g.Key,
                    Total = g.Count(),
                    Ocupadas = g.Count(h => habitacionesOcupadas.Contains(h.HabitacionId))
                })
                .ToList()
        };
    }

    private async Task<ReservasStatsDto> GetReservasStats(
        GetDashboardReservasStatsQuery request,
        CancellationToken cancellationToken)
    {
        var reservas = await _unitOfWork.Reservas.GetAllAsync(cancellationToken);
        var habitaciones = await _unitOfWork.Habitaciones.GetAll();
        var ahora = DateTime.Now;
        var hoy = ahora.Date;
        var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);

        // Crear un diccionario de habitacionId -> hotelId para filtrado
        var habitacionesDict = habitaciones.ToDictionary(h => h.HabitacionId, h => h.HotelId);

        // Filtrar por hotel si se especifica
        if (request.HotelId.HasValue)
        {
            reservas = reservas.Where(r =>
                r.HabitacionId.HasValue &&
                habitacionesDict.ContainsKey(r.HabitacionId.Value) &&
                habitacionesDict[r.HabitacionId.Value] == request.HotelId.Value);
        }

        // Filtrar por rango de fechas si se especifica
        if (request.Desde.HasValue)
        {
            reservas = reservas.Where(r => r.FechaCreacion >= request.Desde.Value);
        }
        if (request.Hasta.HasValue)
        {
            reservas = reservas.Where(r => r.FechaCreacion <= request.Hasta.Value);
        }

        var reservasList = reservas.ToList();

        // Reservas por estado: Pendiente=1, Activa=2, CheckOut=3, Cancelada=4
        var reservasConfirmadas = reservasList.Where(r =>
            r.EstadoReservaId == EstadoReserva.Pendiente).ToList();
        var reservasEnCurso = reservasList.Where(r =>
            r.EstadoReservaId == EstadoReserva.Activa).ToList();
        var reservasActivas = reservasConfirmadas.Concat(reservasEnCurso).ToList();

        // Check-ins de hoy (excluye canceladas)
        var checkInsHoy = reservasList.Where(r =>
            r.FechaCheckIn.Date == hoy &&
            r.CheckInRealizado == null &&
            r.EstadoReservaId != EstadoReserva.Cancelada).ToList();

        // Check-outs de hoy (excluye canceladas)
        var checkOutsHoy = reservasList.Where(r =>
            r.FechaCheckOut.Date == hoy &&
            r.CheckInRealizado != null &&
            r.CheckOutRealizado == null &&
            r.EstadoReservaId != EstadoReserva.Cancelada).ToList();

        // Calcular promedio de estancia
        var reservasCompletadas = reservasList
            .Where(r => r.CheckInRealizado.HasValue && r.CheckOutRealizado.HasValue)
            .ToList();
        var promedioEstancia = reservasCompletadas.Any()
            ? reservasCompletadas.Average(r => (r.FechaCheckOut - r.FechaCheckIn).TotalDays)
            : 0;

        // Crear diccionario de habitaciones para búsqueda rápida
        var habitacionesNumeroDict = habitaciones.ToDictionary(h => h.HabitacionId, h => h.NumeroHabitacion ?? "Sin asignar");

        return new ReservasStatsDto
        {
            Total = reservasList.Count,
            Activas = reservasActivas.Count,
            Confirmadas = reservasConfirmadas.Count,
            EnCurso = reservasEnCurso.Count,
            DelMes = reservasList.Count(r => r.FechaCreacion >= inicioMes),
            CheckInsHoy = checkInsHoy.Count,
            CheckOutsHoy = checkOutsHoy.Count,
            PromedioEstanciaDias = Math.Round((decimal)promedioEstancia, 1),
            IngresosTotales = reservasList.Sum(r => r.MontoTotal),
            IngresosMesActual = reservasList
                .Where(r => r.FechaCreacion >= inicioMes)
                .Sum(r => r.MontoTotal),
            CheckInsDeHoy = checkInsHoy
                .Take(10)
                .Select(r => new CheckInHoyDto
                {
                    ReservaId = r.ReservaId,
                    NumeroReserva = r.NumeroReserva ?? "Sin número",
                    HabitacionNumero = r.HabitacionId.HasValue && habitacionesNumeroDict.ContainsKey(r.HabitacionId.Value)
                        ? habitacionesNumeroDict[r.HabitacionId.Value]
                        : "Sin asignar",
                    HuespedNombre = $"Huésped {r.HuespedId}",
                    Estado = "Pendiente"
                })
                .ToList(),
            CheckOutsDeHoy = checkOutsHoy
                .Take(10)
                .Select(r => new CheckOutHoyDto
                {
                    ReservaId = r.ReservaId,
                    NumeroReserva = r.NumeroReserva ?? "Sin número",
                    HabitacionNumero = r.HabitacionId.HasValue && habitacionesNumeroDict.ContainsKey(r.HabitacionId.Value)
                        ? habitacionesNumeroDict[r.HabitacionId.Value]
                        : "Sin asignar",
                    HuespedNombre = $"Huésped {r.HuespedId}",
                    Estado = "Pendiente"
                })
                .ToList()
        };
    }

    private async Task<ActividadesStatsDto> GetActividadesStats(
        GetDashboardReservasStatsQuery request,
        CancellationToken cancellationToken)
    {
        var actividades = await _unitOfWork.ActividadesRecreativas.GetAllAsync(cancellationToken);
        var reservasActividades = await _unitOfWork.ReservasActividades.GetAllAsync(cancellationToken);
        var ahora = DateTime.Now;
        var hoy = ahora.Date;

        var actividadesList = actividades.ToList();
        var reservasActividadesList = reservasActividades.ToList();

        // Si hay filtro de hotel, filtrar actividades por hotel
        if (request.HotelId.HasValue)
        {
            actividadesList = actividadesList
                .Where(a => a.HotelId == request.HotelId.Value)
                .ToList();

            var actividadesIds = actividadesList.Select(a => a.ActividadId).ToHashSet();
            reservasActividadesList = reservasActividadesList
                .Where(ra => actividadesIds.Contains(ra.ActividadId))
                .ToList();
        }

        var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);

        return new ActividadesStatsDto
        {
            Total = actividadesList.Count,
            Activas = actividadesList.Count(a => a.EstaActiva),
            Inactivas = actividadesList.Count(a => !a.EstaActiva),
            ReservasHoy = reservasActividadesList.Count(ra => ra.FechaReserva.Date == hoy),
            ReservasTotal = reservasActividadesList.Count,
            TopActividades = actividadesList
                .Select(a => new TopActividadDto
                {
                    ActividadId = a.ActividadId,
                    Nombre = a.NombreActividad ?? "Sin nombre",
                    Categoria = a.Categoria ?? "Sin categoría",
                    ReservasTotal = reservasActividadesList.Count(ra => ra.ActividadId == a.ActividadId),
                    ReservasMes = reservasActividadesList.Count(ra =>
                        ra.ActividadId == a.ActividadId &&
                        ra.FechaReserva >= inicioMes)
                })
                .OrderByDescending(a => a.ReservasTotal)
                .Take(5)
                .ToList()
        };
    }
}
