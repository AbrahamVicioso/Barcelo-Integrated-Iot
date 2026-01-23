-- =============================================
-- JOBS Y MANTENIMIENTO AUTOMATIZADO
-- =============================================

-- SP: Job diario de limpieza de logs antiguos
CREATE PROCEDURE [dbo].[SP_LimpiarLogsAntiguos]
    @DiasRetencion INT = 90
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @FechaCorte DATETIME2 = DATEADD(DAY, -@DiasRetencion, GETUTCDATE());
    
    -- Limpiar logs del sistema
    DELETE FROM [dbo].[LogsSistema]
    WHERE FechaHora < @FechaCorte
      AND Nivel NOT IN ('Error', 'Fatal');
    
    DECLARE @LogsEliminados INT = @@ROWCOUNT;
    
    -- Limpiar registros de acceso antiguos (mantener solo resumen)
    DELETE FROM [dbo].[RegistrosAcceso]
    WHERE FechaHoraAcceso < @FechaCorte
      AND FueExitoso = 1;
    
    DECLARE @AccesosEliminados INT = @@ROWCOUNT;
    
    SELECT 
        @LogsEliminados AS LogsEliminados,
        @AccesosEliminados AS AccesosEliminados;
END