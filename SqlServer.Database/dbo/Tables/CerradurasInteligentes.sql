CREATE TABLE [dbo].[CerradurasInteligentes](
	[CerraduraId] [int] IDENTITY(1,1) NOT NULL,
	[DispositivoId] [int] NOT NULL,
	[HabitacionId] [int] NOT NULL,
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
 CONSTRAINT [UQ_Cerraduras_Habitacion] UNIQUE NONCLUSTERED 
(
	[HabitacionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
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
/****** Object:  Index [IX_Cerraduras_DispositivoId]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Cerraduras_DispositivoId] ON [dbo].[CerradurasInteligentes]
(
	[DispositivoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Cerraduras_EstadoPuerta]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Cerraduras_EstadoPuerta] ON [dbo].[CerradurasInteligentes]
(
	[EstadoPuerta] ASC,
	[EstaActiva] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Cerraduras_HabitacionId]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Cerraduras_HabitacionId] ON [dbo].[CerradurasInteligentes]
(
	[HabitacionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]