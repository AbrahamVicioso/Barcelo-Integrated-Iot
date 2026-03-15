-- =============================================================
-- Seed: EstadosDispositivo
-- Datos iniciales para los estados funcionales de dispositivos
-- =============================================================

IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadosDispositivo])
BEGIN
    SET IDENTITY_INSERT [dbo].[EstadosDispositivo] OFF;

    INSERT INTO [dbo].[EstadosDispositivo] ([EstadoDispositivoId], [Descripcion]) VALUES
        (1, N'Operativo'),
        (2, N'Mantenimiento'),
        (3, N'Falla');
END
GO
