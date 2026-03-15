-- **VISTA ACTUALIZADA: Estado de Cerraduras y Dispositivos**
CREATE VIEW [dbo].[VW_EstadoCerraduras]
AS
SELECT 
    cer.CerraduraId,
    disp.DispositivoId,
    disp.NumeroSerieDispositivo,
    disp.DireccionMAC,
    tip.Nombre AS TipoDispositivo,
    disp.Modelo,
    disp.VersionFirmware,
    disp.NivelBateria,
    disp.EstaEnLinea,
    disp.UltimaSincronizacion,
    disp.EstadoDispositivoId AS EstadoFuncionalDispositivo,
    cer.EstadoPuerta,
    cer.UltimaApertura,
    cer.ContadorAperturas,
    hot.Nombre AS NombreHotel,
    hab.NumeroHabitacion,
    tiph.Nombre AS TipoHabitacion,
    CASE 
        WHEN disp.NivelBateria < 20 THEN 'Critico'
        WHEN disp.NivelBateria < 40 THEN 'Bajo'
        WHEN disp.NivelBateria < 70 THEN 'Medio'
        ELSE 'Alto'
    END AS EstadoBateria,
    CASE 
        WHEN disp.EstaEnLinea = 0 THEN 'Offline'
        WHEN DATEDIFF(MINUTE, disp.UltimaSincronizacion, GETUTCDATE()) > 30 THEN 'Desactualizada'
        ELSE 'Online'
    END AS EstadoConexion
FROM [dbo].[CerradurasInteligentes] cer
INNER JOIN [dbo].[Dispositivos]    disp ON cer.DispositivoId    = disp.DispositivoId
INNER JOIN [dbo].[TiposDispositivo] tip  ON disp.TipoDispositivoId = tip.TipoDispositivoId
INNER JOIN [dbo].[Habitaciones]    hab  ON cer.HabitacionId       = hab.HabitacionId
INNER JOIN [dbo].[TiposHabitacion] tiph ON hab.TipoHabitacionId  = tiph.TipoHabitacionId
INNER JOIN [dbo].[Hoteles]         hot  ON hab.HotelId            = hot.HotelId;