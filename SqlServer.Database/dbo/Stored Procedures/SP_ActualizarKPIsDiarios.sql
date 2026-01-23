-- SP: Actualizar KPIs diarios
CREATE PROCEDURE [dbo].[SP_ActualizarKPIsDiarios]
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @FechaMedicion DATETIME2 = GETUTCDATE();
    DECLARE @FechaHoy DATE = CAST(@FechaMedicion AS DATE);
    
    -- Por cada hotel activo
    DECLARE @HotelId INT;
    
    DECLARE hotel_cursor CURSOR FOR
    SELECT HotelId FROM [dbo].[Hoteles] WHERE EstaActivo = 1;
    
    OPEN hotel_cursor;
    FETCH NEXT FROM hotel_cursor INTO @HotelId;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- KPI: Tasa de ocupaciÃ³n
        INSERT INTO [dbo].[IndicadoresKPI] (
            HotelId, NombreMetrica, ValorMetrica, FechaMedicion, 
            Categoria, UnidadMedida, Periodo
        )
        SELECT 
            @HotelId,
            'TasaOcupacion' AS NombreMetrica,
            [dbo].[FN_CalcularTasaOcupacion](@HotelId, @FechaHoy) AS ValorMetrica,
            @FechaMedicion,
            'Ocupacion' AS Categoria,
            'Porcentaje' AS UnidadMedida,
            'Diario' AS Periodo;
        
        -- KPI: Total de accesos del dÃ­a
        INSERT INTO [dbo].[IndicadoresKPI] (
            HotelId, NombreMetrica, ValorMetrica, FechaMedicion,
            Categoria, UnidadMedida, Periodo
        )
        SELECT 
            @HotelId,
            'TotalAccesos' AS NombreMetrica,
            COUNT(*) AS ValorMetrica,
            @FechaMedicion,
            'Accesos' AS Categoria,
            'Cantidad' AS UnidadMedida,
            'Diario' AS Periodo
        FROM [dbo].[RegistrosAcceso] ra
        INNER JOIN [dbo].[CerradurasInteligentes] cer ON ra.CerraduraId = cer.CerraduraId
        INNER JOIN [dbo].[Habitaciones] hab ON cer.HabitacionId = hab.HabitacionId
        WHERE hab.HotelId = @HotelId
          AND CAST(ra.FechaHoraAcceso AS DATE) = @FechaHoy;
        
        -- KPI: Actividades reservadas del dÃ­a
        INSERT INTO [dbo].[IndicadoresKPI] (
            HotelId, NombreMetrica, ValorMetrica, FechaMedicion,
            Categoria, UnidadMedida, Periodo
        )
        SELECT 
            @HotelId,
            'ActividadesReservadas' AS NombreMetrica,
            COUNT(*) AS ValorMetrica,
            @FechaMedicion,
            'Actividades' AS Categoria,
            'Cantidad' AS UnidadMedida,
            'Diario' AS Periodo
        FROM [dbo].[ReservasActividades] ra
        INNER JOIN [dbo].[ActividadesRecreativas] act ON ra.ActividadId = act.ActividadId
        WHERE act.HotelId = @HotelId
          AND ra.FechaReserva = @FechaHoy;
        
        -- KPI: Incidentes de seguridad
        INSERT INTO [dbo].[IndicadoresKPI] (
            HotelId, NombreMetrica, ValorMetrica, FechaMedicion,
            Categoria, UnidadMedida, Periodo
        )
        SELECT 
            @HotelId,
            'IncidentesSeguridad' AS NombreMetrica,
            COUNT(*) AS ValorMetrica,
            @FechaMedicion,
            'Seguridad' AS Categoria,
            'Cantidad' AS UnidadMedida,
            'Diario' AS Periodo
        FROM [dbo].[EventosSistema]
        WHERE HotelId = @HotelId
          AND Severidad IN ('Error', 'Critico')
          AND CAST(FechaOcurrencia AS DATE) = @FechaHoy;
        
        -- **NUEVO KPI: Salud de dispositivos IoT**
        INSERT INTO [dbo].[IndicadoresKPI] (
            HotelId, NombreMetrica, ValorMetrica, FechaMedicion,
            Categoria, UnidadMedida, Periodo
        )
        SELECT 
            @HotelId,
            'DispositivosOperativos' AS NombreMetrica,
            CAST(SUM(CASE WHEN EstadoFuncional = 'Operativo' AND EstaEnLinea = 1 THEN 1 ELSE 0 END) AS FLOAT) / 
            CAST(COUNT(*) AS FLOAT) * 100 AS ValorMetrica,
            @FechaMedicion,
            'Eficiencia' AS Categoria,
            'Porcentaje' AS UnidadMedida,
            'Diario' AS Periodo
        FROM [dbo].[Dispositivos]
        WHERE HotelId = @HotelId;
        
        FETCH NEXT FROM hotel_cursor INTO @HotelId;
    END
    
    CLOSE hotel_cursor;
    DEALLOCATE hotel_cursor;
END