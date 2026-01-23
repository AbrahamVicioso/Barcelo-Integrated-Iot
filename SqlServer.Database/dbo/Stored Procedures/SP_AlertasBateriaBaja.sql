-- **SP ACTUALIZADO: Alertas de dispositivos con baterÃ­a baja**
CREATE PROCEDURE [dbo].[SP_AlertasBateriaBaja]
    @UmbralBateria INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        disp.DispositivoId,
        disp.NumeroSerieDispositivo,
        disp.TipoDispositivo,
        disp.NivelBateria,
        disp.EstadoFuncional,
        hot.Nombre AS NombreHotel,
        CASE 
            WHEN cer.CerraduraId IS NOT NULL THEN hab.NumeroHabitacion
            ELSE 'N/A'
        END AS Ubicacion,
        disp.UltimaSincronizacion
    FROM [dbo].[Dispositivos] disp
    INNER JOIN [dbo].[Hoteles] hot ON disp.HotelId = hot.HotelId
    LEFT JOIN [dbo].[CerradurasInteligentes] cer ON disp.DispositivoId = cer.DispositivoId
    LEFT JOIN [dbo].[Habitaciones] hab ON cer.HabitacionId = hab.HabitacionId
    WHERE disp.NivelBateria <= @UmbralBateria
      AND disp.EstadoFuncional = 'Operativo'
    ORDER BY disp.NivelBateria ASC;
END