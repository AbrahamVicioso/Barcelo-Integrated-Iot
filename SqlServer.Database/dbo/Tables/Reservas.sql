CREATE TABLE [dbo].[Reservas](
	[ReservaId] [int] IDENTITY(1,1) NOT NULL,
	[HuespedId] [int] NOT NULL,
	[HabitacionId] [int] NOT NULL,
	[NumeroReserva] [nvarchar](50) NOT NULL,
	[FechaCheckIn] [datetime2](7) NOT NULL,
	[FechaCheckOut] [datetime2](7) NOT NULL,
	[NumeroHuespedes] [int] NOT NULL,
	[NumeroNinos] [int] NOT NULL,
	[MontoTotal] [decimal](10, 2) NOT NULL,
	[MontoPagado] [decimal](10, 2) NOT NULL,
	[Estado] [nvarchar](30) NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[FechaActualizacion] [datetime2](7) NULL,
	[CheckInRealizado] [datetime2](7) NULL,
	[CheckOutRealizado] [datetime2](7) NULL,
	[CreadoPor] [nvarchar](450) NULL,
	[Observaciones] [nvarchar](1000) NULL,
 CONSTRAINT [PK_Reservas] PRIMARY KEY CLUSTERED 
(
	[ReservaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Reservas_NumeroReserva] UNIQUE NONCLUSTERED 
(
	[NumeroReserva] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Reservas]  WITH CHECK ADD  CONSTRAINT [FK_Reservas_Habitaciones] FOREIGN KEY([HabitacionId])
REFERENCES [dbo].[Habitaciones] ([HabitacionId])
GO

ALTER TABLE [dbo].[Reservas] CHECK CONSTRAINT [FK_Reservas_Habitaciones]
GO
ALTER TABLE [dbo].[Reservas]  WITH CHECK ADD  CONSTRAINT [FK_Reservas_Huespedes] FOREIGN KEY([HuespedId])
REFERENCES [dbo].[Huespedes] ([HuespedId])
GO

ALTER TABLE [dbo].[Reservas] CHECK CONSTRAINT [FK_Reservas_Huespedes]
GO
ALTER TABLE [dbo].[Reservas] ADD  DEFAULT ((1)) FOR [NumeroHuespedes]
GO
ALTER TABLE [dbo].[Reservas] ADD  DEFAULT ((0)) FOR [NumeroNinos]
GO
ALTER TABLE [dbo].[Reservas] ADD  DEFAULT ((0)) FOR [MontoPagado]
GO
ALTER TABLE [dbo].[Reservas] ADD  DEFAULT ('Pendiente') FOR [Estado]
GO
ALTER TABLE [dbo].[Reservas] ADD  DEFAULT (getutcdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Reservas]  WITH CHECK ADD  CONSTRAINT [CHK_Reservas_Fechas] CHECK  (([FechaCheckOut]>[FechaCheckIn]))
GO

ALTER TABLE [dbo].[Reservas] CHECK CONSTRAINT [CHK_Reservas_Fechas]
GO
/****** Object:  Index [IX_Reservas_Estado_Fechas]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Reservas_Estado_Fechas] ON [dbo].[Reservas]
(
	[Estado] ASC,
	[FechaCheckIn] ASC,
	[FechaCheckOut] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Reservas_HabitacionId]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Reservas_HabitacionId] ON [dbo].[Reservas]
(
	[HabitacionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Reservas_HuespedId]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Reservas_HuespedId] ON [dbo].[Reservas]
(
	[HuespedId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Reservas_NumeroReserva]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Reservas_NumeroReserva] ON [dbo].[Reservas]
(
	[NumeroReserva] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]