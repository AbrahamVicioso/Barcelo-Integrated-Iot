-- SP: Obtener KPIs del hotel
CREATE PROCEDURE [dbo].[SP_ObtenerKPIsHotel]
    @HotelId INT,
    @FechaInicio DATETIME2,
    @FechaFin DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Tasa de ocupaciÃ³n
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
      AND GETUTCDATE() BETWEEN r.FechaCheckIn AND r.FechaCheckOut;
    
    SET @TasaOcupacion = CASE 
        WHEN @TotalHabitaciones > 0 THEN (@HabitacionesOcupadas * 100.0) / @TotalHabitaciones
        ELSE 0
    END;
    
    -- Total de accesos
    DECLARE @TotalAccesos INT;
    SELECT @TotalAccesos = COUNT(*)
    FROM [dbo].[RegistrosAcceso] ra
    INNER JOIN [dbo].[CerradurasInteligentes] cer ON ra.CerraduraId = cer.CerraduraId
    INNER JOIN [dbo].[Habitaciones] hab ON cer.HabitacionId = hab.HabitacionId
    WHERE hab.HotelId = @HotelId
      AND ra.FechaHoraAcceso BETWEEN @FechaInicio AND @FechaFin;
    
    -- Actividades reservadas
    DECLARE @ActividadesReservadas INT;
    SELECT @ActividadesReservadas = COUNT(*)
    FROM [dbo].[ReservasActividades] ra
    INNER JOIN [dbo].[ActividadesRecreativas] act ON ra.ActividadId = act.ActividadId
    WHERE act.HotelId = @HotelId
      AND ra.FechaReserva BETWEEN CAST(@FechaInicio AS DATE) AND CAST(@FechaFin AS DATE);
    
    -- Alertas de seguridad
    DECLARE @AlertasSeguridad INT;
    SELECT @AlertasSeguridad = COUNT(*)
    FROM [dbo].[EventosSistema]
    WHERE HotelId = @HotelId
      AND Severidad IN ('Error', 'Critico')
      AND FechaOcurrencia BETWEEN @FechaInicio AND @FechaFin
      AND EstaResuelto = 0;
    
    -- Dispositivos con problemas
    DECLARE @DispositivosProblema INT;
    SELECT @DispositivosProblema = COUNT(*)
    FROM [dbo].[Dispositivos]
    WHERE HotelId = @HotelId
      AND (EstadoFuncional != 'Operativo' OR NivelBateria < 20 OR EstaEnLinea = 0);
    
    -- Retornar resultados
    SELECT 
        @TasaOcupacion AS TasaOcupacion,
        @TotalAccesos AS TotalAccesos,
        @ActividadesReservadas AS ActividadesReservadas,
        @AlertasSeguridad AS AlertasSeguridad,
        @TotalHabitaciones AS TotalHabitaciones,
        @HabitacionesOcupadas AS HabitacionesOcupadas,
        @DispositivosProblema AS DispositivosConProblemas;
END