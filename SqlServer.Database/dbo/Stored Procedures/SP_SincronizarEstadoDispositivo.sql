-- **NUEVO SP: Sincronizar estado de dispositivo**
CREATE PROCEDURE [dbo].[SP_SincronizarEstadoDispositivo]
    @DispositivoId INT,
    @NivelBateria INT,
    @EstaEnLinea BIT,
    @VersionFirmware NVARCHAR(20) = NULL,
    @IPDispositivo NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE [dbo].[Dispositivos]
        SET NivelBateria = @NivelBateria,
            EstaEnLinea = @EstaEnLinea,
            UltimaSincronizacion = GETUTCDATE(),
            VersionFirmware = COALESCE(@VersionFirmware, VersionFirmware),
            IPDispositivo = COALESCE(@IPDispositivo, IPDispositivo),
            FechaActualizacion = GETUTCDATE()
        WHERE DispositivoId = @DispositivoId;
        
        -- Generar alerta si baterÃ­a estÃ¡ baja
        IF @NivelBateria < 20 AND NOT EXISTS (
            SELECT 1 FROM [dbo].[EventosSistema]
            WHERE DispositivoId = @DispositivoId
              AND TipoEvento = 'BateriaBaja'
              AND EstaResuelto = 0
        )
        BEGIN
            DECLARE @HotelId INT;
            DECLARE @NumeroSerie NVARCHAR(100);
            
            SELECT @HotelId = HotelId, @NumeroSerie = NumeroSerieDispositivo
            FROM [dbo].[Dispositivos]
            WHERE DispositivoId = @DispositivoId;
            
            INSERT INTO [dbo].[EventosSistema] (
                TipoEvento, Severidad, Descripcion, Origen, DispositivoId, HotelId
            )
            VALUES (
                'BateriaBaja',
                CASE WHEN @NivelBateria < 10 THEN 'Critico' ELSE 'Advertencia' END,
                'BaterÃ­a baja en dispositivo ' + @NumeroSerie + ' - Nivel: ' + CAST(@NivelBateria AS NVARCHAR(10)) + '%',
                'Sistema de Monitoreo IoT',
                @DispositivoId,
                @HotelId
            );
        END
        
        SELECT 'OK' AS Resultado;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END