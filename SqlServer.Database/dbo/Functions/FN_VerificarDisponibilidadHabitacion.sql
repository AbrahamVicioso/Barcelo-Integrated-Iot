-- Función: Verificar disponibilidad de habitación
CREATE FUNCTION [dbo].[FN_VerificarDisponibilidadHabitacion]
(
    @HabitacionId INT,
    @FechaInicio  DATETIME2,
    @FechaFin     DATETIME2
)
RETURNS BIT
AS
BEGIN
    DECLARE @Disponible BIT = 1;

    -- EstadoReservaId IN (1=Pendiente, 2=Activa) bloquean disponibilidad
    IF EXISTS (
        SELECT 1
        FROM [dbo].[Reservas]
        WHERE HabitacionId = @HabitacionId
          AND EstadoReservaId IN (1, 2)
          AND (
              (@FechaInicio BETWEEN FechaCheckIn AND FechaCheckOut) OR
              (@FechaFin    BETWEEN FechaCheckIn AND FechaCheckOut) OR
              (FechaCheckIn BETWEEN @FechaInicio AND @FechaFin)
          )
    )
    BEGIN
        SET @Disponible = 0;
    END

    RETURN @Disponible;
END
