-- Migration: Replace Puesto/Departamento string columns on Personal with FK references
-- Run after creating Puestos and Departamentos tables

-- 1. Create Puestos table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Puestos]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Puestos](
        [PuestoId] [int] IDENTITY(1,1) NOT NULL,
        [Nombre] [nvarchar](100) NOT NULL,
        [Descripcion] [nvarchar](500) NULL,
        [EstaActivo] [bit] NOT NULL DEFAULT ((1)),
        [FechaCreacion] [datetime2](7) NOT NULL DEFAULT (getutcdate()),
        [EliminadoEn] [datetime2](7) NULL,
        CONSTRAINT [PK_Puestos] PRIMARY KEY CLUSTERED ([PuestoId] ASC),
        CONSTRAINT [UQ_Puestos_Nombre] UNIQUE NONCLUSTERED ([Nombre] ASC)
    )
    PRINT 'Tabla Puestos creada.'
END
GO

-- 2. Create Departamentos table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Departamentos]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Departamentos](
        [DepartamentoId] [int] IDENTITY(1,1) NOT NULL,
        [Nombre] [nvarchar](100) NOT NULL,
        [Descripcion] [nvarchar](500) NULL,
        [EstaActivo] [bit] NOT NULL DEFAULT ((1)),
        [FechaCreacion] [datetime2](7) NOT NULL DEFAULT (getutcdate()),
        [EliminadoEn] [datetime2](7) NULL,
        CONSTRAINT [PK_Departamentos] PRIMARY KEY CLUSTERED ([DepartamentoId] ASC),
        CONSTRAINT [UQ_Departamentos_Nombre] UNIQUE NONCLUSTERED ([Nombre] ASC)
    )
    PRINT 'Tabla Departamentos creada.'
END
GO

-- 3. Migrate existing Puesto values into Puestos table
INSERT INTO [dbo].[Puestos] ([Nombre])
SELECT DISTINCT [Puesto]
FROM [dbo].[Personal]
WHERE [Puesto] IS NOT NULL AND [Puesto] != ''
  AND NOT EXISTS (SELECT 1 FROM [dbo].[Puestos] WHERE [Nombre] = [Personal].[Puesto])
GO

-- 4. Migrate existing Departamento values into Departamentos table
INSERT INTO [dbo].[Departamentos] ([Nombre])
SELECT DISTINCT [Departamento]
FROM [dbo].[Personal]
WHERE [Departamento] IS NOT NULL AND [Departamento] != ''
  AND NOT EXISTS (SELECT 1 FROM [dbo].[Departamentos] WHERE [Nombre] = [Personal].[Departamento])
GO

-- 5. Add PuestoId column to Personal (nullable first)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Personal]') AND name = 'PuestoId')
BEGIN
    ALTER TABLE [dbo].[Personal] ADD [PuestoId] [int] NULL
    PRINT 'Columna PuestoId agregada a Personal.'
END
GO

-- 6. Add DepartamentoId column to Personal (nullable first)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Personal]') AND name = 'DepartamentoId')
BEGIN
    ALTER TABLE [dbo].[Personal] ADD [DepartamentoId] [int] NULL
    PRINT 'Columna DepartamentoId agregada a Personal.'
END
GO

-- 7. Populate PuestoId from existing Puesto string
UPDATE p
SET p.[PuestoId] = pu.[PuestoId]
FROM [dbo].[Personal] p
INNER JOIN [dbo].[Puestos] pu ON pu.[Nombre] = p.[Puesto]
GO

-- 8. Populate DepartamentoId from existing Departamento string
UPDATE p
SET p.[DepartamentoId] = d.[DepartamentoId]
FROM [dbo].[Personal] p
INNER JOIN [dbo].[Departamentos] d ON d.[Nombre] = p.[Departamento]
GO

-- 9. Make PuestoId and DepartamentoId NOT NULL
ALTER TABLE [dbo].[Personal] ALTER COLUMN [PuestoId] [int] NOT NULL
GO
ALTER TABLE [dbo].[Personal] ALTER COLUMN [DepartamentoId] [int] NOT NULL
GO

-- 10. Add FK constraints
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Personal_Puestos')
BEGIN
    ALTER TABLE [dbo].[Personal] ADD CONSTRAINT [FK_Personal_Puestos]
        FOREIGN KEY ([PuestoId]) REFERENCES [dbo].[Puestos] ([PuestoId])
    ALTER TABLE [dbo].[Personal] CHECK CONSTRAINT [FK_Personal_Puestos]
    PRINT 'FK_Personal_Puestos agregado.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Personal_Departamentos')
BEGIN
    ALTER TABLE [dbo].[Personal] ADD CONSTRAINT [FK_Personal_Departamentos]
        FOREIGN KEY ([DepartamentoId]) REFERENCES [dbo].[Departamentos] ([DepartamentoId])
    ALTER TABLE [dbo].[Personal] CHECK CONSTRAINT [FK_Personal_Departamentos]
    PRINT 'FK_Personal_Departamentos agregado.'
END
GO

-- 11. Drop old string columns (only after verifying data is migrated)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Personal]') AND name = 'Puesto')
BEGIN
    ALTER TABLE [dbo].[Personal] DROP COLUMN [Puesto]
    PRINT 'Columna Puesto eliminada de Personal.'
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Personal]') AND name = 'Departamento')
BEGIN
    ALTER TABLE [dbo].[Personal] DROP COLUMN [Departamento]
    PRINT 'Columna Departamento eliminada de Personal.'
END
GO

PRINT 'Migración completada exitosamente.'
GO
