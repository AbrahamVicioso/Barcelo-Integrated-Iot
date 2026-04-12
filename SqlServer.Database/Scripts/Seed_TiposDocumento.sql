-- Seed: TiposDocumento
-- Inserta tipos de documento iniciales solo si la tabla está vacía

IF NOT EXISTS (SELECT TOP 1 1 FROM [dbo].[TiposDocumento])
BEGIN
    SET IDENTITY_INSERT [dbo].[TiposDocumento] ON;

    INSERT INTO [dbo].[TiposDocumento] ([TipoDocumentoId], [Nombre], [Descripcion], [EstaActivo], [FechaCreacion])
    VALUES
        (1,  'Pasaporte',                 'Documento de viaje internacional emitido por el gobierno',                    1, GETUTCDATE()),
        (2,  'Cédula de Identidad',       'Documento de identidad nacional emitido por el gobierno',                    1, GETUTCDATE()),
        (3,  'Licencia de Conducir',      'Documento habilitante para conducir vehículos, válido como identificación',  1, GETUTCDATE());

    SET IDENTITY_INSERT [dbo].[TiposDocumento] OFF;

    PRINT 'Seed de TiposDocumento completado: 8 registros insertados.'
END
ELSE
BEGIN
    PRINT 'La tabla TiposDocumento ya contiene datos. Seed omitido.'
END
GO
