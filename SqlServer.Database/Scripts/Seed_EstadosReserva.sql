-- =============================================================
-- Seed: EstadosReserva
-- Datos iniciales para los estados de reservas
-- =============================================================

IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadosReserva])
BEGIN
    INSERT INTO [dbo].[EstadosReserva] ([EstadoReservaId], [Nombre], [Descripcion]) VALUES
        (1, N'Pendiente', N'Reserva creada, pendiente de check-in'),
        (2, N'Activa',    N'Check-in realizado, huésped en el hotel'),
        (3, N'CheckOut',  N'Check-out realizado, reserva finalizada'),
        (4, N'Cancelada', N'Reserva cancelada');
END
GO
