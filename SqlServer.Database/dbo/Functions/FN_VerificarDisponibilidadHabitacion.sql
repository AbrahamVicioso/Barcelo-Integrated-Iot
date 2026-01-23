-- FunciÃ³n: Verificar disponibilidad de habitaciÃ³n
CREATE FUNCTION [dbo].[FN_VerificarDisponibilidadHabitacion]
(
    @HabitacionId INT,
    @FechaInicio DATETIME2,
    @FechaFin DATETIME2
)
RETURNS BIT
AS
BEGIN
    DECLARE @Disponible BIT = 1;
    
    IF EXISTS (
        SELECT 1
        FROM [dbo].[Reservas]
        WHERE HabitacionId = @HabitacionId
          AND Estado IN ('Confirmada', 'CheckInRealizado')
          AND (
              (@FechaInicio BETWEEN FechaCheckIn AND FechaCheckOut) OR
              (@FechaFin BETWEEN FechaCheckIn AND FechaCheckOut) OR
              (FechaCheckIn BETWEEN @FechaInicio AND @FechaFin)
          )
    )
    BEGIN
        SET @Disponible = 0;
    END
    
    RETURN @Disponible;
END