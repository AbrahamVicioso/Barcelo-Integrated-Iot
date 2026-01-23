CREATE TABLE [dbo].[IndicadoresKPI](
	[KPIId] [bigint] IDENTITY(1,1) NOT NULL,
	[HotelId] [int] NOT NULL,
	[NombreMetrica] [nvarchar](100) NOT NULL,
	[ValorMetrica] [decimal](18, 4) NOT NULL,
	[FechaMedicion] [datetime2](7) NOT NULL,
	[Categoria] [nvarchar](50) NOT NULL,
	[UnidadMedida] [nvarchar](50) NULL,
	[MetaObjetivo] [decimal](18, 4) NULL,
	[Periodo] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_IndicadoresKPI] PRIMARY KEY CLUSTERED 
(
	[KPIId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[IndicadoresKPI]  WITH CHECK ADD  CONSTRAINT [FK_IndicadoresKPI_Hoteles] FOREIGN KEY([HotelId])
REFERENCES [dbo].[Hoteles] ([HotelId])
GO

ALTER TABLE [dbo].[IndicadoresKPI] CHECK CONSTRAINT [FK_IndicadoresKPI_Hoteles]
GO
ALTER TABLE [dbo].[IndicadoresKPI] ADD  DEFAULT (getutcdate()) FOR [FechaMedicion]
GO
/****** Object:  Index [IX_IndicadoresKPI_Categoria]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_IndicadoresKPI_Categoria] ON [dbo].[IndicadoresKPI]
(
	[Categoria] ASC,
	[NombreMetrica] ASC,
	[FechaMedicion] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_IndicadoresKPI_HotelId_Fecha]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_IndicadoresKPI_HotelId_Fecha] ON [dbo].[IndicadoresKPI]
(
	[HotelId] ASC,
	[FechaMedicion] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]