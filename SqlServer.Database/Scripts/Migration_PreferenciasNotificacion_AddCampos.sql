-- Script de migración para agregar nuevos campos a PreferenciasNotificacion
-- Fecha: 2026-04-21

-- Verificar si la columna ya existe antes de agregar
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PreferenciasNotificacion') AND name = 'NotificarReservas')
BEGIN
    ALTER TABLE [dbo].[PreferenciasNotificacion] ADD [NotificarReservas] [bit] NOT NULL DEFAULT ((1));
    PRINT 'Columna NotificarReservas agregada';
END
ELSE
BEGIN
    PRINT 'Columna NotificarReservas ya existe';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PreferenciasNotificacion') AND name = 'NotificarCredenciales')
BEGIN
    ALTER TABLE [dbo].[PreferenciasNotificacion] ADD [NotificarCredenciales] [bit] NOT NULL DEFAULT ((1));
    PRINT 'Columna NotificarCredenciales agregada';
END
ELSE
BEGIN
    PRINT 'Columna NotificarCredenciales ya existe';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PreferenciasNotificacion') AND name = 'NotificarCheckIn')
BEGIN
    ALTER TABLE [dbo].[PreferenciasNotificacion] ADD [NotificarCheckIn] [bit] NOT NULL DEFAULT ((1));
    PRINT 'Columna NotificarCheckIn agregada';
END
ELSE
BEGIN
    PRINT 'Columna NotificarCheckIn ya existe';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PreferenciasNotificacion') AND name = 'NotificarCuentaCreada')
BEGIN
    ALTER TABLE [dbo].[PreferenciasNotificacion] ADD [NotificarCuentaCreada] [bit] NOT NULL DEFAULT ((1));
    PRINT 'Columna NotificarCuentaCreada agregada';
END
ELSE
BEGIN
    PRINT 'Columna NotificarCuentaCreada ya existe';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PreferenciasNotificacion') AND name = 'NotificarConfirmacionEmail')
BEGIN
    ALTER TABLE [dbo].[PreferenciasNotificacion] ADD [NotificarConfirmacionEmail] [bit] NOT NULL DEFAULT ((1));
    PRINT 'Columna NotificarConfirmacionEmail agregada';
END
ELSE
BEGIN
    PRINT 'Columna NotificarConfirmacionEmail ya existe';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PreferenciasNotificacion') AND name = 'NotificarRestablecerPassword')
BEGIN
    ALTER TABLE [dbo].[PreferenciasNotificacion] ADD [NotificarRestablecerPassword] [bit] NOT NULL DEFAULT ((1));
    PRINT 'Columna NotificarRestablecerPassword agregada';
END
ELSE
BEGIN
    PRINT 'Columna NotificarRestablecerPassword ya existe';
END
GO

PRINT 'Migración completada exitosamente';
GO
