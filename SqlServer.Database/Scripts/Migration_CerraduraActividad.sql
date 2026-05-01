-- Migration: Add activity smart lock support
-- Applies to existing databases.
-- Run once. Safe to inspect before executing.

-- 1. Make HabitacionId nullable in CerradurasInteligentes
ALTER TABLE [dbo].[CerradurasInteligentes] ALTER COLUMN [HabitacionId] INT NULL;
GO

-- 2. Drop the non-filtered unique constraint (does not support WHERE clause)
ALTER TABLE [dbo].[CerradurasInteligentes] DROP CONSTRAINT [UQ_Cerraduras_Habitacion];
GO

-- 3. Add ActividadId column
ALTER TABLE [dbo].[CerradurasInteligentes] ADD [ActividadId] INT NULL;
GO

-- 4. Add CHECK constraint: a lock must belong to a room OR an activity
ALTER TABLE [dbo].[CerradurasInteligentes] ADD CONSTRAINT [CHK_Cerraduras_Contexto]
    CHECK ([HabitacionId] IS NOT NULL OR [ActividadId] IS NOT NULL);
GO

-- 5. Add FK to ActividadesRecreativas
ALTER TABLE [dbo].[CerradurasInteligentes] WITH CHECK
    ADD CONSTRAINT [FK_Cerraduras_Actividades] FOREIGN KEY ([ActividadId])
    REFERENCES [dbo].[ActividadesRecreativas] ([ActividadId]);
GO
ALTER TABLE [dbo].[CerradurasInteligentes] CHECK CONSTRAINT [FK_Cerraduras_Actividades];
GO

-- 6. Recreate HabitacionId unique index as filtered (allows multiple NULLs)
DROP INDEX IF EXISTS [IX_Cerraduras_HabitacionId] ON [dbo].[CerradurasInteligentes];
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Cerraduras_Habitacion]
    ON [dbo].[CerradurasInteligentes] ([HabitacionId])
    WHERE ([HabitacionId] IS NOT NULL);
GO
CREATE NONCLUSTERED INDEX [IX_Cerraduras_HabitacionId]
    ON [dbo].[CerradurasInteligentes] ([HabitacionId])
    WHERE ([HabitacionId] IS NOT NULL);
GO

-- 7. Add filtered unique index for ActividadId
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Cerraduras_Actividad]
    ON [dbo].[CerradurasInteligentes] ([ActividadId])
    WHERE ([ActividadId] IS NOT NULL);
GO
CREATE NONCLUSTERED INDEX [IX_Cerraduras_ActividadId]
    ON [dbo].[CerradurasInteligentes] ([ActividadId])
    WHERE ([ActividadId] IS NOT NULL);
GO

-- 8. Add ReservaActividadId to CredencialesAcceso
ALTER TABLE [dbo].[CredencialesAcceso] ADD [ReservaActividadId] INT NULL;
GO

-- 9. FK to ReservasActividades
ALTER TABLE [dbo].[CredencialesAcceso] WITH CHECK
    ADD CONSTRAINT [FK_Credenciales_ReservasActividades] FOREIGN KEY ([ReservaActividadId])
    REFERENCES [dbo].[ReservasActividades] ([ReservaActividadId]);
GO
ALTER TABLE [dbo].[CredencialesAcceso] CHECK CONSTRAINT [FK_Credenciales_ReservasActividades];
GO

-- 10. Filtered index for ReservaActividadId
CREATE NONCLUSTERED INDEX [IX_Credenciales_ReservaActividadId]
    ON [dbo].[CredencialesAcceso] ([ReservaActividadId])
    WHERE ([ReservaActividadId] IS NOT NULL);
GO
