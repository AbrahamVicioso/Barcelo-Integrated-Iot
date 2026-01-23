-- Vista de Accesos Recientes
CREATE VIEW [dbo].[VW_AccesosRecientes]
AS
SELECT TOP 1000
    ra.RegistroId,
    ra.FechaHoraAcceso,
    ra.TipoAcceso,
    ra.ResultadoAcceso,
    ra.FueExitoso,
    ra.MotivoAcceso,
    u.UserName AS Usuario,
    u.Email,
    hot.Nombre AS NombreHotel,
    hab.NumeroHabitacion,
    disp.NumeroSerieDispositivo,
    cred.CodigoPIN,
    cred.TipoCredencial
FROM [dbo].[RegistrosAcceso] ra
INNER JOIN [dbo].[CerradurasInteligentes] cer ON ra.CerraduraId = cer.CerraduraId
INNER JOIN [dbo].[Dispositivos] disp ON cer.DispositivoId = disp.DispositivoId
INNER JOIN [dbo].[Habitaciones] hab ON cer.HabitacionId = hab.HabitacionId
INNER JOIN [dbo].[Hoteles] hot ON hab.HotelId = hot.HotelId
LEFT JOIN [dbo].[CredencialesAcceso] cred ON ra.CredencialId = cred.CredencialId
LEFT JOIN [dbo].[Users] u ON ra.UsuarioId = u.Id
ORDER BY ra.FechaHoraAcceso DESC;