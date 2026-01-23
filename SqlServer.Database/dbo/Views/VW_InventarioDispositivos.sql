-- **NUEVA VISTA: Inventario de Dispositivos IoT**
CREATE VIEW [dbo].[VW_InventarioDispositivos]
AS
SELECT 
    disp.DispositivoId,
    disp.NumeroSerieDispositivo,
    disp.DireccionMAC,
    disp.TipoDispositivo,
    disp.Modelo,
    disp.VersionFirmware,
    disp.NivelBateria,
    disp.EstaEnLinea,
    disp.EstadoFuncional,
    disp.FechaInstalacion,
    disp.UltimaActualizacionFirmware,
    hot.Nombre AS NombreHotel,
    hot.Ciudad,
    CASE 
        WHEN cer.CerraduraId IS NOT NULL THEN hab.NumeroHabitacion
        ELSE NULL
    END AS UbicacionEspecifica,
    DATEDIFF(DAY, disp.FechaInstalacion, GETUTCDATE()) AS DiasInstalado,
    CASE 
        WHEN disp.UltimaActualizacionFirmware IS NULL THEN 'Sin Actualizar'
        WHEN DATEDIFF(DAY, disp.UltimaActualizacionFirmware, GETUTCDATE()) > 180 THEN 'Desactualizado'
        ELSE 'Actualizado'
    END AS EstadoFirmware
FROM [dbo].[Dispositivos] disp
INNER JOIN [dbo].[Hoteles] hot ON disp.HotelId = hot.HotelId
LEFT JOIN [dbo].[CerradurasInteligentes] cer ON disp.DispositivoId = cer.DispositivoId
LEFT JOIN [dbo].[Habitaciones] hab ON cer.HabitacionId = hab.HabitacionId;