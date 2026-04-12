-- =============================================================
-- Seed: TiposDispositivo
-- Datos iniciales para los tipos de dispositivos IoT del hotel
-- =============================================================

IF NOT EXISTS (SELECT 1 FROM [dbo].[TiposDispositivo])
BEGIN
    INSERT INTO [dbo].[TiposDispositivo] ([TipoDispositivoId], [Nombre]) VALUES
        (1, N'Cerradura Inteligente');
END
GO
