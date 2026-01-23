-- Vista: Top actividades mÃ¡s reservadas
CREATE VIEW [dbo].[VW_ActividadesPopulares]
AS
SELECT 
    act.ActividadId,
    act.NombreActividad,
    act.Categoria,
    hot.Nombre AS NombreHotel,
    COUNT(ra.ReservaActividadId) AS TotalReservas,
    AVG(CAST(ra.NumeroPersonas AS FLOAT)) AS PromedioPersonas,
    SUM(ra.MontoTotal) AS IngresoTotal
FROM [dbo].[ActividadesRecreativas] act
INNER JOIN [dbo].[Hoteles] hot ON act.HotelId = hot.HotelId
LEFT JOIN [dbo].[ReservasActividades] ra ON act.ActividadId = ra.ActividadId
WHERE ra.Estado IN ('Confirmada', 'Completada')
GROUP BY 
    act.ActividadId, act.NombreActividad, 
    act.Categoria, hot.Nombre;