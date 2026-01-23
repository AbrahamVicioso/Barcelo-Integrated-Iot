CREATE TABLE [dbo].[ReservasActividades](
	[ReservaActividadId] [int] IDENTITY(1,1) NOT NULL,
	[ActividadId] [int] NOT NULL,
	[HuespedId] [int] NOT NULL,
	[FechaReserva] [date] NOT NULL,
	[HoraReserva] [time](7) NOT NULL,
	[NumeroPersonas] [int] NOT NULL,
	[Estado] [nvarchar](30) NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[MontoTotal] [decimal](10, 2) NOT NULL,
	[NotasEspeciales] [nvarchar](500) NULL,
	[RecordatorioEnviado] [bit] NOT NULL,
	[FechaRecordatorio] [datetime2](7) NULL,
 CONSTRAINT [PK_ReservasActividades] PRIMARY KEY CLUSTERED 
(
	[ReservaActividadId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ReservasActividades]  WITH CHECK ADD  CONSTRAINT [FK_ReservasActividades_Actividades] FOREIGN KEY([ActividadId])
REFERENCES [dbo].[ActividadesRecreativas] ([ActividadId])
GO

ALTER TABLE [dbo].[ReservasActividades] CHECK CONSTRAINT [FK_ReservasActividades_Actividades]
GO
ALTER TABLE [dbo].[ReservasActividades]  WITH CHECK ADD  CONSTRAINT [FK_ReservasActividades_Huespedes] FOREIGN KEY([HuespedId])
REFERENCES [dbo].[Huespedes] ([HuespedId])
GO

ALTER TABLE [dbo].[ReservasActividades] CHECK CONSTRAINT [FK_ReservasActividades_Huespedes]
GO
ALTER TABLE [dbo].[ReservasActividades] ADD  DEFAULT ((1)) FOR [NumeroPersonas]
GO
ALTER TABLE [dbo].[ReservasActividades] ADD  DEFAULT ('Confirmada') FOR [Estado]
GO
ALTER TABLE [dbo].[ReservasActividades] ADD  DEFAULT (getutcdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[ReservasActividades] ADD  DEFAULT ((0)) FOR [RecordatorioEnviado]
GO
/****** Object:  Index [IX_ReservasActividades_Fecha]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_ReservasActividades_Fecha] ON [dbo].[ReservasActividades]
(
	[FechaReserva] ASC,
	[HoraReserva] ASC
)
INCLUDE([ActividadId],[Estado]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ReservasActividades_HuespedId]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_ReservasActividades_HuespedId] ON [dbo].[ReservasActividades]
(
	[HuespedId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]