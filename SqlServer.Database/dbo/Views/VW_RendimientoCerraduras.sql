-- =============================================
-- VISTAS ADICIONALES PARA ANÃLISIS
-- =============================================

-- Vista: Resumen de rendimiento de cerraduras
CREATE VIEW [dbo].[VW_RendimientoCerraduras]
AS
SELECT 
    cer.CerraduraId,
    disp.NumeroSerieDispositivo,
    hot.Nombre AS NombreHotel,
    hab.NumeroHabitacion,
    cer.ContadorAperturas,
    COUNT(ra.RegistroId) AS TotalAccesos,
    SUM(CASE WHEN ra.FueExitoso = 1 THEN 1 ELSE 0 END) AS AccesosExitosos,
    SUM(CASE WHEN ra.FueExitoso = 0 THEN 1 ELSE 0 END) AS AccesosFallidos,
    AVG(CAST(ra.Latencia AS FLOAT)) AS LatenciaPromedioAccesos,
    MAX(ra.FechaHoraAcceso) AS UltimoAcceso,
    disp.NivelBateria,
    disp.EstadoFuncional
FROM [dbo].[CerradurasInteligentes] cer
INNER JOIN [dbo].[Dispositivos] disp ON cer.DispositivoId = disp.DispositivoId
INNER JOIN [dbo].[Habitaciones] hab ON cer.HabitacionId = hab.HabitacionId
INNER JOIN [dbo].[Hoteles] hot ON hab.HotelId = hot.HotelId
LEFT JOIN [dbo].[RegistrosAcceso] ra ON cer.CerraduraId = ra.CerraduraId
GROUP BY 
    cer.CerraduraId, disp.NumeroSerieDispositivo, 
    hot.Nombre, hab.NumeroHabitacion,
    cer.ContadorAperturas,
    disp.NivelBateria, disp.EstadoFuncional;