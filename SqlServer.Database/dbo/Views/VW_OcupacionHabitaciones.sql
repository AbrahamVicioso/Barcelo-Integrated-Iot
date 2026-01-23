-- =============================================
-- VISTAS PARA REPORTES Y CONSULTAS COMUNES
-- =============================================

-- Vista de OcupaciÃ³n de Habitaciones
CREATE VIEW [dbo].[VW_OcupacionHabitaciones]
AS
SELECT 
    h.HotelId,
    hot.Nombre AS NombreHotel,
    h.HabitacionId,
    h.NumeroHabitacion,
    h.TipoHabitacion,
    h.Estado,
    h.EstaDisponible,
    r.ReservaId,
    r.NumeroReserva,
    r.FechaCheckIn,
    r.FechaCheckOut,
    r.Estado AS EstadoReserva,
    hues.NombreCompleto AS NombreHuesped,
    CASE 
        WHEN r.Estado = 'CheckInRealizado' AND GETUTCDATE() BETWEEN r.FechaCheckIn AND r.FechaCheckOut THEN 1
        ELSE 0
    END AS EstaOcupada
FROM [dbo].[Habitaciones] h
INNER JOIN [dbo].[Hoteles] hot ON h.HotelId = hot.HotelId
LEFT JOIN [dbo].[Reservas] r ON h.HabitacionId = r.HabitacionId 
    AND r.Estado IN ('Confirmada', 'CheckInRealizado')
    AND GETUTCDATE() BETWEEN r.FechaCheckIn AND r.FechaCheckOut
LEFT JOIN [dbo].[Huespedes] hues ON r.HuespedId = hues.HuespedId;