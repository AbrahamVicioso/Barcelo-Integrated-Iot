-- =============================================
-- FUNCIONES ÃšTILES
-- =============================================

-- FunciÃ³n: Calcular tasa de ocupaciÃ³n
CREATE FUNCTION [dbo].[FN_CalcularTasaOcupacion]
(
    @HotelId INT,
    @Fecha DATE
)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @TasaOcupacion DECIMAL(10,2);
    DECLARE @TotalHabitaciones INT;
    DECLARE @HabitacionesOcupadas INT;
    
    SELECT @TotalHabitaciones = COUNT(*)
    FROM [dbo].[Habitaciones]
    WHERE HotelId = @HotelId AND EstaDisponible = 1;
    
    SELECT @HabitacionesOcupadas = COUNT(DISTINCT r.HabitacionId)
    FROM [dbo].[Reservas] r
    INNER JOIN [dbo].[Habitaciones] h ON r.HabitacionId = h.HabitacionId
    WHERE h.HotelId = @HotelId
      AND r.Estado = 'CheckInRealizado'
      AND @Fecha BETWEEN CAST(r.FechaCheckIn AS DATE) AND CAST(r.FechaCheckOut AS DATE);
    
    SET @TasaOcupacion = CASE 
        WHEN @TotalHabitaciones > 0 THEN (@HabitacionesOcupadas * 100.0) / @TotalHabitaciones
        ELSE 0
    END;
    
    RETURN @TasaOcupacion;
END