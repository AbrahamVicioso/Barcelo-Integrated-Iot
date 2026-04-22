-- Manual migration for EstadoReservaActividad
-- Run this if the table doesn't exist

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EstadosReservaActividad]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EstadosReservaActividad](
        [EstadoReservaActividadId] [int] NOT NULL,
        [Nombre] [nvarchar](50) NOT NULL,
        [Descripcion] [nvarchar](200) NULL,
        CONSTRAINT [PK_EstadosReservaActividad] PRIMARY KEY CLUSTERED ([EstadoReservaActividadId] ASC)
    )
END
GO

-- Seed data
IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadosReservaActividad])
BEGIN
    INSERT INTO [dbo].[EstadosReservaActividad] ([EstadoReservaActividadId], [Nombre], [Descripcion]) VALUES
        (1, N'Pendiente',   N'Reserva de actividad pendiente de confirmación'),
        (2, N'Confirmada',  N'Reserva de actividad confirmada'),
        (3, N'Cancelada',   N'Reserva de actividad cancelada');
END
GO

-- Add FK to ReservasActividades if not exists
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ReservasActividades_EstadosReservaActividad]') AND parent_object_id = OBJECT_ID(N'[dbo].[ReservasActividades]'))
BEGIN
    -- Add column if doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ReservasActividades]') AND name = 'EstadoReservaActividadId')
    BEGIN
        ALTER TABLE [dbo].[ReservasActividades]
        ADD [EstadoReservaActividadId] [int] NOT NULL DEFAULT(2)
    END

    -- Add FK
    ALTER TABLE [dbo].[ReservasActividades]
    WITH CHECK ADD CONSTRAINT [FK_ReservasActividades_EstadosReservaActividad]
    FOREIGN KEY([EstadoReservaActividadId])
    REFERENCES [dbo].[EstadosReservaActividad] ([EstadoReservaActividadId])
    ON DELETE NO ACTION
END
GO
