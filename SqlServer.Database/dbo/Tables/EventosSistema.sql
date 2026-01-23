CREATE TABLE [dbo].[EventosSistema](
	[EventoId] [bigint] IDENTITY(1,1) NOT NULL,
	[TipoEvento] [nvarchar](100) NOT NULL,
	[Severidad] [nvarchar](20) NOT NULL,
	[Descripcion] [nvarchar](2000) NOT NULL,
	[Origen] [nvarchar](200) NOT NULL,
	[FechaOcurrencia] [datetime2](7) NOT NULL,
	[EstaResuelto] [bit] NOT NULL,
	[ResueltoPor] [nvarchar](450) NULL,
	[FechaResolucion] [datetime2](7) NULL,
	[SolucionAplicada] [nvarchar](1000) NULL,
	[DispositivoId] [int] NULL,
	[CerraduraId] [int] NULL,
	[HotelId] [int] NULL,
 CONSTRAINT [PK_EventosSistema] PRIMARY KEY CLUSTERED 
(
	[EventoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[EventosSistema]  WITH CHECK ADD  CONSTRAINT [FK_EventosSistema_Cerraduras] FOREIGN KEY([CerraduraId])
REFERENCES [dbo].[CerradurasInteligentes] ([CerraduraId])
GO

ALTER TABLE [dbo].[EventosSistema] CHECK CONSTRAINT [FK_EventosSistema_Cerraduras]
GO
ALTER TABLE [dbo].[EventosSistema]  WITH CHECK ADD  CONSTRAINT [FK_EventosSistema_Dispositivos] FOREIGN KEY([DispositivoId])
REFERENCES [dbo].[Dispositivos] ([DispositivoId])
GO

ALTER TABLE [dbo].[EventosSistema] CHECK CONSTRAINT [FK_EventosSistema_Dispositivos]
GO
ALTER TABLE [dbo].[EventosSistema]  WITH CHECK ADD  CONSTRAINT [FK_EventosSistema_Hoteles] FOREIGN KEY([HotelId])
REFERENCES [dbo].[Hoteles] ([HotelId])
GO

ALTER TABLE [dbo].[EventosSistema] CHECK CONSTRAINT [FK_EventosSistema_Hoteles]
GO
ALTER TABLE [dbo].[EventosSistema] ADD  DEFAULT (getutcdate()) FOR [FechaOcurrencia]
GO
ALTER TABLE [dbo].[EventosSistema] ADD  DEFAULT ((0)) FOR [EstaResuelto]
GO
/****** Object:  Index [IX_EventosSistema_CerraduraId]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_EventosSistema_CerraduraId] ON [dbo].[EventosSistema]
(
	[CerraduraId] ASC
)
WHERE ([CerraduraId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_EventosSistema_DispositivoId]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_EventosSistema_DispositivoId] ON [dbo].[EventosSistema]
(
	[DispositivoId] ASC
)
WHERE ([DispositivoId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_EventosSistema_NoResueltos]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_EventosSistema_NoResueltos] ON [dbo].[EventosSistema]
(
	[EstaResuelto] ASC,
	[Severidad] ASC,
	[FechaOcurrencia] DESC
)
WHERE ([EstaResuelto]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]