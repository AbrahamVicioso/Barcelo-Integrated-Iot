CREATE TABLE [dbo].[CerradurasInteligentes](
	[CerraduraId] [int] IDENTITY(1,1) NOT NULL,
	[DispositivoId] UNIQUEIDENTIFIER NOT NULL,
	[HabitacionId] [int] NULL,
	[ActividadId] [int] NULL,
	[EstadoPuerta] [nvarchar](20) NOT NULL,
	[UltimaApertura] [datetime2](7) NULL,
	[ContadorAperturas] [int] NOT NULL,
	[SoportaModoOffline] [bit] NOT NULL,
	[FechaActivacion] [datetime2](7) NOT NULL,
	[EstaActiva] [bit] NOT NULL,
 CONSTRAINT [PK_CerradurasInteligentes] PRIMARY KEY CLUSTERED
(
	[CerraduraId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [CHK_Cerraduras_Contexto] CHECK ([HabitacionId] IS NOT NULL OR [ActividadId] IS NOT NULL)
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CerradurasInteligentes]  WITH CHECK ADD  CONSTRAINT [FK_Cerraduras_Dispositivos] FOREIGN KEY([DispositivoId])
REFERENCES [dbo].[Dispositivos] ([DispositivoId])
GO

ALTER TABLE [dbo].[CerradurasInteligentes] CHECK CONSTRAINT [FK_Cerraduras_Dispositivos]
GO
ALTER TABLE [dbo].[CerradurasInteligentes]  WITH CHECK ADD  CONSTRAINT [FK_Cerraduras_Habitaciones] FOREIGN KEY([HabitacionId])
REFERENCES [dbo].[Habitaciones] ([HabitacionId])
GO

ALTER TABLE [dbo].[CerradurasInteligentes] CHECK CONSTRAINT [FK_Cerraduras_Habitaciones]
GO
ALTER TABLE [dbo].[CerradurasInteligentes]  WITH CHECK ADD  CONSTRAINT [FK_Cerraduras_Actividades] FOREIGN KEY([ActividadId])
REFERENCES [dbo].[ActividadesRecreativas] ([ActividadId])
GO

ALTER TABLE [dbo].[CerradurasInteligentes] CHECK CONSTRAINT [FK_Cerraduras_Actividades]
GO
ALTER TABLE [dbo].[CerradurasInteligentes] ADD  DEFAULT ('Cerrada') FOR [EstadoPuerta]
GO
ALTER TABLE [dbo].[CerradurasInteligentes] ADD  DEFAULT ((0)) FOR [ContadorAperturas]
GO
ALTER TABLE [dbo].[CerradurasInteligentes] ADD  DEFAULT ((1)) FOR [SoportaModoOffline]
GO
ALTER TABLE [dbo].[CerradurasInteligentes] ADD  DEFAULT (getutcdate()) FOR [FechaActivacion]
GO
ALTER TABLE [dbo].[CerradurasInteligentes] ADD  DEFAULT ((1)) FOR [EstaActiva]
GO
/****** Object:  Index [IX_Cerraduras_DispositivoId] ******/
CREATE NONCLUSTERED INDEX [IX_Cerraduras_DispositivoId] ON [dbo].[CerradurasInteligentes]
(
	[DispositivoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Cerraduras_EstadoPuerta] ******/
CREATE NONCLUSTERED INDEX [IX_Cerraduras_EstadoPuerta] ON [dbo].[CerradurasInteligentes]
(
	[EstadoPuerta] ASC,
	[EstaActiva] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Cerraduras_HabitacionId] ******/
CREATE NONCLUSTERED INDEX [IX_Cerraduras_HabitacionId] ON [dbo].[CerradurasInteligentes]
(
	[HabitacionId] ASC
)
WHERE ([HabitacionId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ_Cerraduras_Habitacion] ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Cerraduras_Habitacion] ON [dbo].[CerradurasInteligentes]
(
	[HabitacionId] ASC
)
WHERE ([HabitacionId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Cerraduras_ActividadId] ******/
CREATE NONCLUSTERED INDEX [IX_Cerraduras_ActividadId] ON [dbo].[CerradurasInteligentes]
(
	[ActividadId] ASC
)
WHERE ([ActividadId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ_Cerraduras_Actividad] ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Cerraduras_Actividad] ON [dbo].[CerradurasInteligentes]
(
	[ActividadId] ASC
)
WHERE ([ActividadId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
