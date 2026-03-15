-- =============================================================
-- Seed: TiposDispositivo
-- Datos iniciales para los tipos de dispositivos IoT del hotel
-- =============================================================

IF NOT EXISTS (SELECT 1 FROM [dbo].[TiposDispositivo])
BEGIN
    INSERT INTO [dbo].[TiposDispositivo] ([TipoDispositivoId], [Nombre]) VALUES
        (1, N'Cerradura Inteligente'),
        (2, N'Termostato'),
        (3, N'Sensor de Movimiento'),
        (4, N'Camara'),
        (5, N'Control de Iluminacion'),
        (6, N'Sensor de Temperatura'),
        (7, N'Panel de Control');
END
GO
