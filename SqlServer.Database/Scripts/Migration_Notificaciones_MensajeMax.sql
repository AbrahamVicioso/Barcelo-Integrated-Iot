IF COL_LENGTH('dbo.Notificaciones', 'Mensaje') < 8000 OR COL_LENGTH('dbo.Notificaciones', 'Mensaje') IS NULL
BEGIN
    ALTER TABLE [dbo].[Notificaciones] ALTER COLUMN [Mensaje] [nvarchar](max) NOT NULL;
END
GO
