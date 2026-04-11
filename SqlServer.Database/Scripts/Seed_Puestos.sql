-- Seed: Puestos
-- Inserta puestos iniciales solo si la tabla está vacía

IF NOT EXISTS (SELECT TOP 1 1 FROM [dbo].[Puestos])
BEGIN
    SET IDENTITY_INSERT [dbo].[Puestos] ON;

    INSERT INTO [dbo].[Puestos] ([PuestoId], [Nombre], [Descripcion], [EstaActivo], [FechaCreacion])
    VALUES
        (1,  'Gerente General',         'Responsable de la dirección y operación global del hotel',                     1, GETUTCDATE()),
        (2,  'Subgerente',              'Apoyo a la gerencia general y supervisión de áreas operativas',                1, GETUTCDATE()),
        (3,  'Recepcionista',           'Atención al huésped, registro de entrada/salida y gestión de reservas',        1, GETUTCDATE()),
        (4,  'Conserje',                'Asistencia a huéspedes, información turística y coordinación de servicios',    1, GETUTCDATE()),
        (5,  'Ama de Llaves',           'Supervisión del servicio de limpieza de habitaciones y áreas comunes',        1, GETUTCDATE()),
        (6,  'Camarero/a de Piso',      'Limpieza y preparación de habitaciones asignadas',                            1, GETUTCDATE()),
        (7,  'Técnico de Mantenimiento','Reparación y mantenimiento preventivo de instalaciones y equipos',             1, GETUTCDATE()),
        (8,  'Chef Ejecutivo',          'Dirección de cocina, menús y control de calidad gastronómica',                 1, GETUTCDATE()),
        (9,  'Cocinero/a',              'Preparación de alimentos según los estándares del establecimiento',            1, GETUTCDATE()),
        (10, 'Mesero/a',                'Servicio de alimentos y bebidas en restaurante o eventos',                    1, GETUTCDATE()),
        (11, 'Bartender',               'Preparación y servicio de bebidas en el bar del hotel',                       1, GETUTCDATE()),
        (12, 'Agente de Seguridad',     'Vigilancia, control de acceso y respuesta ante incidentes',                   1, GETUTCDATE()),
        (13, 'Coordinador de Ventas',   'Gestión de cuentas corporativas, eventos y paquetes turísticos',               1, GETUTCDATE()),
        (14, 'Contador/a',              'Registro contable, facturación y control financiero',                         1, GETUTCDATE()),
        (15, 'Especialista en TI',      'Administración de sistemas, infraestructura IoT y soporte técnico',            1, GETUTCDATE());

    SET IDENTITY_INSERT [dbo].[Puestos] OFF;

    PRINT 'Seed de Puestos completado: 15 registros insertados.'
END
ELSE
BEGIN
    PRINT 'La tabla Puestos ya contiene datos. Seed omitido.'
END
GO
