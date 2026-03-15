-- =============================================================
-- Seed: TiposHabitacion
-- Datos iniciales para los tipos de habitacion del hotel
-- =============================================================

IF NOT EXISTS (SELECT 1 FROM [dbo].[TiposHabitacion])
BEGIN
    INSERT INTO [dbo].[TiposHabitacion] ([TipoHabitacionId], [Nombre]) VALUES
        (1, N'Simple'),
        (2, N'Doble'),
        (3, N'Triple'),
        (4, N'Suite'),
        (5, N'Suite Junior'),
        (6, N'Suite Presidencial'),
        (7, N'Familiar');
END
GO
