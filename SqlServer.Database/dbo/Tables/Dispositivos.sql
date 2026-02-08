CREATE TABLE [dbo].[Dispositivos](
	[DispositivoId] UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
	[HotelId] [int] NOT NULL,
	[NumeroSerieDispositivo] [nvarchar](100) NOT NULL,
	[DireccionMAC] [nvarchar](50) NOT NULL,
	[TipoDispositivo] [nvarchar](50) NOT NULL,
	[Modelo] [nvarchar](100) NULL,
	[VersionFirmware] [nvarchar](20) NOT NULL,
	[NivelBateria] [int] NOT NULL,
	[EstaEnLinea] [bit] NOT NULL,
	[UltimaSincronizacion] [datetime2](7) NULL,
	[FechaInstalacion] [datetime2](7) NOT NULL,
	[EstadoFuncional] [nvarchar](20) NOT NULL,
	[UltimaActualizacionFirmware] [datetime2](7) NULL,
	[IPDispositivo] [nvarchar](50) NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[FechaActualizacion] [datetime2](7) NULL,
 CONSTRAINT [PK_Dispositivos] PRIMARY KEY CLUSTERED 
(
	[DispositivoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Dispositivos_MAC] UNIQUE NONCLUSTERED 
(
	[DireccionMAC] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Dispositivos_NumeroSerie] UNIQUE NONCLUSTERED 
(
	[NumeroSerieDispositivo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Dispositivos]  WITH CHECK ADD  CONSTRAINT [FK_Dispositivos_Hoteles] FOREIGN KEY([HotelId])
REFERENCES [dbo].[Hoteles] ([HotelId])
GO

ALTER TABLE [dbo].[Dispositivos] CHECK CONSTRAINT [FK_Dispositivos_Hoteles]
GO
ALTER TABLE [dbo].[Dispositivos] ADD  DEFAULT ((100)) FOR [NivelBateria]
GO
ALTER TABLE [dbo].[Dispositivos] ADD  DEFAULT ((1)) FOR [EstaEnLinea]
GO
ALTER TABLE [dbo].[Dispositivos] ADD  DEFAULT (getutcdate()) FOR [FechaInstalacion]
GO
ALTER TABLE [dbo].[Dispositivos] ADD  DEFAULT ('Operativo') FOR [EstadoFuncional]
GO
ALTER TABLE [dbo].[Dispositivos] ADD  DEFAULT (getutcdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Dispositivos]  WITH CHECK ADD  CONSTRAINT [CHK_Dispositivos_Bateria] CHECK  (([NivelBateria]>=(0) AND [NivelBateria]<=(100)))
GO

ALTER TABLE [dbo].[Dispositivos] CHECK CONSTRAINT [CHK_Dispositivos_Bateria]
GO
/****** Object:  Index [IX_Dispositivos_EstadoBateria]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Dispositivos_EstadoBateria] ON [dbo].[Dispositivos]
(
	[NivelBateria] ASC,
	[EstaEnLinea] ASC
)
WHERE ([NivelBateria]<(30))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Dispositivos_HotelId]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Dispositivos_HotelId] ON [dbo].[Dispositivos]
(
	[HotelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Dispositivos_NumeroSerie]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Dispositivos_NumeroSerie] ON [dbo].[Dispositivos]
(
	[NumeroSerieDispositivo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Dispositivos_TipoDispositivo]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Dispositivos_TipoDispositivo] ON [dbo].[Dispositivos]
(
	[TipoDispositivo] ASC,
	[EstadoFuncional] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]