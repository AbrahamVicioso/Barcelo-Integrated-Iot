-- Vista de Ocupación de Habitaciones
CREATE VIEW [dbo].[VW_OcupacionHabitaciones]
AS
SELECT
    h.HotelId,
    hot.Nombre          AS NombreHotel,
    h.HabitacionId,
    h.NumeroHabitacion,
    tip.Nombre          AS TipoHabitacion,
    h.EstadoHabitacionId,
    r.ReservaId,
    r.NumeroReserva,
    r.FechaCheckIn,
    r.FechaCheckOut,
    er.Nombre           AS EstadoReserva,
    hues.NombreCompleto AS NombreHuesped,
    CASE
        WHEN r.EstadoReservaId = 2 AND GETUTCDATE() BETWEEN r.FechaCheckIn AND r.FechaCheckOut THEN 1
        ELSE 0
    END AS EstaOcupada
FROM [dbo].[Habitaciones] h
INNER JOIN [dbo].[TiposHabitacion]  tip  ON h.TipoHabitacionId  = tip.TipoHabitacionId
INNER JOIN [dbo].[Hoteles]          hot  ON h.HotelId            = hot.HotelId
LEFT JOIN  [dbo].[Reservas]         r    ON h.HabitacionId       = r.HabitacionId
    -- EstadoReservaId IN (1=Pendiente, 2=Activa)
    AND r.EstadoReservaId IN (1, 2)
    AND GETUTCDATE() BETWEEN r.FechaCheckIn AND r.FechaCheckOut
LEFT JOIN  [dbo].[EstadosReserva]   er   ON r.EstadoReservaId    = er.EstadoReservaId
LEFT JOIN  [dbo].[Huespedes]        hues ON r.HuespedId           = hues.HuespedId;
