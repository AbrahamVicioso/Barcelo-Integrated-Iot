-- =============================================================
-- Seed: EstadosReservaActividad
-- Datos iniciales para los estados de reservas de actividades
-- =============================================================

IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadosReservaActividad])
BEGIN
    INSERT INTO [dbo].[EstadosReservaActividad] ([EstadoReservaActividadId], [Nombre], [Descripcion]) VALUES
        (1, N'Pendiente',   N'Reserva de actividad pendiente de confirmación'),
        (2, N'Confirmada',  N'Reserva de actividad confirmada'),
        (3, N'Cancelada',   N'Reserva de actividad cancelada');
END
GO
