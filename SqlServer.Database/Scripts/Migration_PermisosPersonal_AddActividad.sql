-- Migración: PermisosPersonal — quitar TipoPermiso, agregar ActividadId
-- Ejecutar una sola vez sobre la BD existente

-- 1. Agregar la nueva columna ActividadId
ALTER TABLE [dbo].[PermisosPersonal]
    ADD [ActividadId] [int] NULL;
GO

-- 2. Agregar FK a ActividadesRecreativas
ALTER TABLE [dbo].[PermisosPersonal]
    WITH CHECK ADD CONSTRAINT [FK_PermisosPersonal_ActividadesRecreativas]
    FOREIGN KEY([ActividadId]) REFERENCES [dbo].[ActividadesRecreativas] ([ActividadId]);
GO

ALTER TABLE [dbo].[PermisosPersonal] CHECK CONSTRAINT [FK_PermisosPersonal_ActividadesRecreativas];
GO

-- 3. Eliminar la columna TipoPermiso
ALTER TABLE [dbo].[PermisosPersonal]
    DROP COLUMN [TipoPermiso];
GO
