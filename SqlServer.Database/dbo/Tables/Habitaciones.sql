CREATE TABLE [dbo].[Habitaciones](
	[HabitacionId] [int] IDENTITY(1,1) NOT NULL,
	[HotelId] [int] NOT NULL,
	[NumeroHabitacion] [nvarchar](20) NOT NULL,
	[TipoHabitacionId] INT NOT NULL,
	[Piso] [int] NOT NULL,
	[CapacidadMaxima] [int] NOT NULL,
	[PrecioPorNoche] [decimal](10, 2) NOT NULL,
	[EstadoHabitacionId] INT NOT NULL,
	[Descripcion] [nvarchar](1000) NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[FechaActualizacion] [datetime2](7) NULL,
 CONSTRAINT [PK_Habitaciones] PRIMARY KEY CLUSTERED 
(
	[HabitacionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Habitacion_Hotel] UNIQUE NONCLUSTERED 
(
	[HotelId] ASC,
	[NumeroHabitacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY], 
    CONSTRAINT [FK_Habitaciones_EstadoHabitacion] FOREIGN KEY ([EstadoHabitacionId]) REFERENCES [dbo].[EstadosHabitacion] ([EstadoHabitacionId]),
    CONSTRAINT [FK_Habitaciones_TiposHabitacion]  FOREIGN KEY ([TipoHabitacionId])  REFERENCES [dbo].[TiposHabitacion]  ([TipoHabitacionId])
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Habitaciones]  WITH CHECK ADD  CONSTRAINT [FK_Habitaciones_Hoteles] FOREIGN KEY([HotelId])
REFERENCES [dbo].[Hoteles] ([HotelId])
GO

ALTER TABLE [dbo].[Habitaciones] CHECK CONSTRAINT [FK_Habitaciones_Hoteles]
GO
ALTER TABLE [dbo].[Habitaciones] ADD  DEFAULT ((2)) FOR [CapacidadMaxima]
GO

GO

GO
ALTER TABLE [dbo].[Habitaciones] ADD  DEFAULT (getutcdate()) FOR [FechaCreacion]
GO
/****** Object:  Index [IX_Habitaciones_Estado]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Habitaciones_Estado] ON [dbo].[Habitaciones]
(
	[EstadoHabitacionId] ASC
)
INCLUDE([HabitacionId],[NumeroHabitacion]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Habitaciones_HotelId]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Habitaciones_HotelId] ON [dbo].[Habitaciones]
(
	[HotelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]