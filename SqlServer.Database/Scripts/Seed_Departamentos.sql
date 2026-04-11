-- Seed: Departamentos
-- Inserta departamentos iniciales solo si la tabla está vacía

IF NOT EXISTS (SELECT TOP 1 1 FROM [dbo].[Departamentos])
BEGIN
    SET IDENTITY_INSERT [dbo].[Departamentos] ON;

    INSERT INTO [dbo].[Departamentos] ([DepartamentoId], [Nombre], [Descripcion], [EstaActivo], [FechaCreacion])
    VALUES
        (1,  'Recepción',           'Atención al cliente, check-in y check-out de huéspedes',                           1, GETUTCDATE()),
        (2,  'Housekeeping',        'Limpieza y mantenimiento de habitaciones y áreas comunes',                         1, GETUTCDATE()),
        (3,  'Mantenimiento',       'Reparaciones, instalaciones y mantenimiento preventivo del hotel',                 1, GETUTCDATE()),
        (4,  'Alimentos y Bebidas', 'Restaurante, bar, room service y banquetes',                                       1, GETUTCDATE()),
        (5,  'Seguridad',           'Control de acceso, vigilancia y protección de instalaciones',                      1, GETUTCDATE()),
        (6,  'Administración',      'Finanzas, contabilidad, recursos humanos y gestión general',                       1, GETUTCDATE()),
        (7,  'Ventas y Marketing',  'Comercialización, reservas corporativas y promoción del hotel',                    1, GETUTCDATE()),
        (8,  'Spa y Bienestar',     'Servicios de spa, gimnasio y actividades recreativas para huéspedes',              1, GETUTCDATE()),
        (9,  'Tecnología',          'Infraestructura tecnológica, sistemas IoT y soporte informático',                  1, GETUTCDATE()),
        (10, 'Cocina',              'Preparación de alimentos, pastelería y gestión de inventario de cocina',           1, GETUTCDATE());

    SET IDENTITY_INSERT [dbo].[Departamentos] OFF;

    PRINT 'Seed de Departamentos completado: 10 registros insertados.'
END
ELSE
BEGIN
    PRINT 'La tabla Departamentos ya contiene datos. Seed omitido.'
END
GO
