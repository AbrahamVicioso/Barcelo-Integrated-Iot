-- SP: Job para enviar recordatorios de actividades
CREATE PROCEDURE [dbo].[SP_EnviarRecordatoriosActividades]
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @FechaHoraLimite DATETIME2 = DATEADD(HOUR, 1, GETUTCDATE());
    
    -- Seleccionar actividades que necesitan recordatorio
    SELECT 
        ra.ReservaActividadId,
        ra.HuespedId,
        h.UsuarioId,
        u.Email,
        act.NombreActividad,
        ra.FechaReserva,
        ra.HoraReserva,
        hot.Nombre AS NombreHotel
    INTO #RecordatoriosPendientes
    FROM [dbo].[ReservasActividades] ra
    INNER JOIN [dbo].[ActividadesRecreativas] act ON ra.ActividadId = act.ActividadId
    INNER JOIN [dbo].[Huespedes] h ON ra.HuespedId = h.HuespedId
    INNER JOIN [dbo].[Users] u ON h.UsuarioId = u.Id
    INNER JOIN [dbo].[Hoteles] hot ON act.HotelId = hot.HotelId
    WHERE ra.Estado = 'Confirmada'
      AND ra.RecordatorioEnviado = 0
      AND CAST(ra.FechaReserva AS DATETIME) + CAST(ra.HoraReserva AS DATETIME) <= @FechaHoraLimite
      AND CAST(ra.FechaReserva AS DATETIME) + CAST(ra.HoraReserva AS DATETIME) > GETUTCDATE();
    
    -- Insertar notificaciones
    INSERT INTO [dbo].[Notificaciones] (
        UsuarioId, TipoNotificacion, Titulo, Mensaje, Prioridad,
        TipoEntidadRelacionada, EntidadRelacionadaId, CanalEnvio
    )
    SELECT 
        rp.UsuarioId,
        'RecordatorioActividad' AS TipoNotificacion,
        'Recordatorio: ' + rp.NombreActividad AS Titulo,
        'Tu actividad ' + rp.NombreActividad + ' estÃ¡ programada para hoy a las ' + 
        CONVERT(NVARCHAR(5), rp.HoraReserva, 108) + ' en ' + rp.NombreHotel AS Mensaje,
        'Media' AS Prioridad,
        'ReservaActividad' AS TipoEntidadRelacionada,
        rp.ReservaActividadId AS EntidadRelacionadaId,
        'Push' AS CanalEnvio
    FROM #RecordatoriosPendientes rp;
    
    -- Marcar recordatorios como enviados
    UPDATE ra
    SET RecordatorioEnviado = 1,
        FechaRecordatorio = GETUTCDATE()
    FROM [dbo].[ReservasActividades] ra
    INNER JOIN #RecordatoriosPendientes rp ON ra.ReservaActividadId = rp.ReservaActividadId;
    
    SELECT @@ROWCOUNT AS RecordatoriosEnviados;
    
    DROP TABLE #RecordatoriosPendientes;
END