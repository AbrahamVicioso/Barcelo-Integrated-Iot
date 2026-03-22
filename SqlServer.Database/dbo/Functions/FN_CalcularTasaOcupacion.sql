-- Función: Calcular tasa de ocupación
CREATE FUNCTION [dbo].[FN_CalcularTasaOcupacion]
(
    @HotelId INT,
    @Fecha   DATE
)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @TasaOcupacion    DECIMAL(10,2);
    DECLARE @TotalHabitaciones INT;
    DECLARE @HabitacionesOcupadas INT;

    SELECT @TotalHabitaciones = COUNT(*)
    FROM [dbo].[Habitaciones]
    WHERE HotelId = @HotelId AND EstadoHabitacionId = 1;

    -- EstadoReservaId = 2 (Activa) significa que el huésped hizo check-in
    SELECT @HabitacionesOcupadas = COUNT(DISTINCT r.HabitacionId)
    FROM [dbo].[Reservas] r
    INNER JOIN [dbo].[Habitaciones] h ON r.HabitacionId = h.HabitacionId
    WHERE h.HotelId = @HotelId
      AND r.EstadoReservaId = 2
      AND @Fecha BETWEEN CAST(r.FechaCheckIn AS DATE) AND CAST(r.FechaCheckOut AS DATE);

    SET @TasaOcupacion = CASE
        WHEN @TotalHabitaciones > 0 THEN (@HabitacionesOcupadas * 100.0) / @TotalHabitaciones
        ELSE 0
    END;

    RETURN @TasaOcupacion;
END
