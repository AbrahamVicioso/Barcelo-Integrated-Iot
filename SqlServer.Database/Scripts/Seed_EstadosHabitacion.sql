-- =============================================================
-- Seed: EstadosHabitacion
-- Datos iniciales para los estados de habitaciones
-- =============================================================

IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadosHabitacion])
BEGIN
    SET IDENTITY_INSERT [dbo].[EstadosHabitacion] OFF;

    INSERT INTO [dbo].[EstadosHabitacion] ([EstadoHabitacionId], [Descripcion]) VALUES
        (1, N'Disponible'),
        (2, N'Ocupada'),
        (3, N'Mantenimiento'),
        (4, N'Fuera de Servicio'),
        (5, N'Limpieza');
END
GO
