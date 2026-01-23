-- Vista de Actividades del DÃ­a
CREATE VIEW [dbo].[VW_ActividadesHoy]
AS
SELECT 
    ra.ReservaActividadId,
    act.NombreActividad,
    act.Categoria,
    act.Ubicacion,
    ra.FechaReserva,
    ra.HoraReserva,
    ra.NumeroPersonas,
    ra.Estado,
    hues.NombreCompleto AS NombreHuesped,
    u.Email,
    u.PhoneNumber,
    hot.Nombre AS NombreHotel,
    ra.RecordatorioEnviado
FROM [dbo].[ReservasActividades] ra
INNER JOIN [dbo].[ActividadesRecreativas] act ON ra.ActividadId = act.ActividadId
INNER JOIN [dbo].[Huespedes] hues ON ra.HuespedId = hues.HuespedId
INNER JOIN [dbo].[Users] u ON hues.UsuarioId = u.Id
INNER JOIN [dbo].[Hoteles] hot ON act.HotelId = hot.HotelId
WHERE ra.FechaReserva = CAST(GETUTCDATE() AS DATE)
  AND ra.Estado = 'Confirmada';