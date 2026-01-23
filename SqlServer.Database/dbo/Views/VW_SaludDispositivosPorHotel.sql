-- **NUEVA VISTA: Resumen de salud de dispositivos por hotel**
CREATE VIEW [dbo].[VW_SaludDispositivosPorHotel]
AS
SELECT 
    hot.HotelId,
    hot.Nombre AS NombreHotel,
    COUNT(disp.DispositivoId) AS TotalDispositivos,
    SUM(CASE WHEN disp.EstadoFuncional = 'Operativo' THEN 1 ELSE 0 END) AS DispositivosOperativos,
    SUM(CASE WHEN disp.EstadoFuncional = 'Mantenimiento' THEN 1 ELSE 0 END) AS DispositivosEnMantenimiento,
    SUM(CASE WHEN disp.EstadoFuncional = 'Falla' THEN 1 ELSE 0 END) AS DispositivosConFalla,
    SUM(CASE WHEN disp.EstaEnLinea = 1 THEN 1 ELSE 0 END) AS DispositivosOnline,
    SUM(CASE WHEN disp.EstaEnLinea = 0 THEN 1 ELSE 0 END) AS DispositivosOffline,
    AVG(CAST(disp.NivelBateria AS FLOAT)) AS NivelBateriaPromedio,
    SUM(CASE WHEN disp.NivelBateria < 20 THEN 1 ELSE 0 END) AS DispositivosBateriasCriticas,
    CAST(SUM(CASE WHEN disp.EstadoFuncional = 'Operativo' AND disp.EstaEnLinea = 1 THEN 1 ELSE 0 END) AS FLOAT) / 
    CAST(COUNT(disp.DispositivoId) AS FLOAT) * 100 AS PorcentajeSaludGeneral
FROM [dbo].[Hoteles] hot
LEFT JOIN [dbo].[Dispositivos] disp ON hot.HotelId = disp.HotelId
GROUP BY hot.HotelId, hot.Nombre;