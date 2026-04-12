-- =============================================================
-- Migration: Huespedes — reemplazar TipoDocumento (nvarchar)
--            por TipoDocumentoId (int FK → TiposDocumento)
-- Ejecutar MANUALMENTE en la base de datos existente.
-- =============================================================

-- Paso 1: Crear la tabla TiposDocumento si no existe
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TiposDocumento' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[TiposDocumento](
        [TipoDocumentoId] [int] IDENTITY(1,1) NOT NULL,
        [Nombre] [nvarchar](100) NOT NULL,
        [Descripcion] [nvarchar](500) NULL,
        [EstaActivo] [bit] NOT NULL DEFAULT ((1)),
        [FechaCreacion] [datetime2](7) NOT NULL DEFAULT (getutcdate()),
        [EliminadoEn] [datetime2](7) NULL,
        CONSTRAINT [PK_TiposDocumento] PRIMARY KEY CLUSTERED ([TipoDocumentoId] ASC),
        CONSTRAINT [UQ_TiposDocumento_Nombre] UNIQUE NONCLUSTERED ([Nombre] ASC)
    );
    PRINT 'Tabla TiposDocumento creada.';
END

-- Paso 2: Insertar tipos de documento base
IF NOT EXISTS (SELECT TOP 1 1 FROM [dbo].[TiposDocumento])
BEGIN
    SET IDENTITY_INSERT [dbo].[TiposDocumento] ON;
    INSERT INTO [dbo].[TiposDocumento] ([TipoDocumentoId], [Nombre], [EstaActivo], [FechaCreacion]) VALUES
        (1, 'Pasaporte',              1, GETUTCDATE()),
        (2, 'DNI',                    1, GETUTCDATE()),
        (3, 'Cédula de Identidad',    1, GETUTCDATE()),
        (4, 'Cédula de Extranjería',  1, GETUTCDATE()),
        (5, 'Tarjeta de Residencia',  1, GETUTCDATE()),
        (6, 'Licencia de Conducir',   1, GETUTCDATE()),
        (7, 'Carné de Identidad',     1, GETUTCDATE()),
        (8, 'Documento de Identidad', 1, GETUTCDATE());
    SET IDENTITY_INSERT [dbo].[TiposDocumento] OFF;
    PRINT 'Seed de TiposDocumento insertado.';
END

-- Paso 3: Agregar columna TipoDocumentoId a Huespedes (nullable primero)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Huespedes') AND name = 'TipoDocumentoId')
BEGIN
    ALTER TABLE [dbo].[Huespedes] ADD [TipoDocumentoId] INT NULL;
    PRINT 'Columna TipoDocumentoId agregada a Huespedes.';
END

-- Paso 4: Mapear valores existentes de TipoDocumento al ID correspondiente
UPDATE h
SET h.TipoDocumentoId = td.TipoDocumentoId
FROM [dbo].[Huespedes] h
JOIN [dbo].[TiposDocumento] td ON LOWER(h.TipoDocumento) LIKE LOWER('%' + td.Nombre + '%')
    OR LOWER(td.Nombre) LIKE LOWER('%' + h.TipoDocumento + '%');

-- Asignar 'Documento de Identidad' a cualquier huésped que no haya mapeado
UPDATE [dbo].[Huespedes]
SET TipoDocumentoId = 8
WHERE TipoDocumentoId IS NULL;

PRINT 'Valores de TipoDocumentoId asignados.';

-- Paso 5: Hacer la columna NOT NULL
ALTER TABLE [dbo].[Huespedes] ALTER COLUMN [TipoDocumentoId] INT NOT NULL;

-- Paso 6: Eliminar constraint unique antiguo
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Huespedes_Documento' AND object_id = OBJECT_ID('dbo.Huespedes'))
BEGIN
    ALTER TABLE [dbo].[Huespedes] DROP CONSTRAINT [UQ_Huespedes_Documento];
    PRINT 'Constraint UQ_Huespedes_Documento eliminado.';
END

-- Paso 7: Agregar nuevo constraint unique con TipoDocumentoId
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Huespedes_Documento' AND object_id = OBJECT_ID('dbo.Huespedes'))
BEGIN
    ALTER TABLE [dbo].[Huespedes]
    ADD CONSTRAINT [UQ_Huespedes_Documento] UNIQUE NONCLUSTERED ([TipoDocumentoId], [NumeroDocumento]);
    PRINT 'Nuevo constraint UQ_Huespedes_Documento (TipoDocumentoId + NumeroDocumento) creado.';
END

-- Paso 8: Agregar FK
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Huespedes_TiposDocumento')
BEGIN
    ALTER TABLE [dbo].[Huespedes]
    ADD CONSTRAINT [FK_Huespedes_TiposDocumento]
    FOREIGN KEY ([TipoDocumentoId]) REFERENCES [dbo].[TiposDocumento]([TipoDocumentoId]);
    PRINT 'FK_Huespedes_TiposDocumento creada.';
END

-- Paso 9: Eliminar columna TipoDocumento antigua
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Huespedes') AND name = 'TipoDocumento')
BEGIN
    ALTER TABLE [dbo].[Huespedes] DROP COLUMN [TipoDocumento];
    PRINT 'Columna TipoDocumento (nvarchar) eliminada.';
END

PRINT 'Migración completada exitosamente.';
GO
