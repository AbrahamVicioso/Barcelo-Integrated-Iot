CREATE TABLE [dbo].[Reportes](
	[ReporteId] [int] IDENTITY(1,1) NOT NULL,
	[NombreReporte] [nvarchar](200) NOT NULL,
	[TipoReporte] [nvarchar](50) NOT NULL,
	[FechaGeneracion] [datetime2](7) NOT NULL,
	[GeneradoPor] [nvarchar](450) NOT NULL,
	[FechaInicio] [datetime2](7) NOT NULL,
	[FechaFin] [datetime2](7) NOT NULL,
	[Formato] [nvarchar](20) NOT NULL,
	[RutaArchivo] [nvarchar](500) NULL,
	[TamanoArchivo] [bigint] NULL,
	[HotelId] [int] NULL,
	[Parametros] [nvarchar](max) NULL,
	[EstadoGeneracion] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_Reportes] PRIMARY KEY CLUSTERED 
(
	[ReporteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Reportes]  WITH CHECK ADD  CONSTRAINT [FK_Reportes_Hoteles] FOREIGN KEY([HotelId])
REFERENCES [dbo].[Hoteles] ([HotelId])
GO

ALTER TABLE [dbo].[Reportes] CHECK CONSTRAINT [FK_Reportes_Hoteles]
GO
ALTER TABLE [dbo].[Reportes]  WITH CHECK ADD  CONSTRAINT [FK_Reportes_Users] FOREIGN KEY([GeneradoPor])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[Reportes] CHECK CONSTRAINT [FK_Reportes_Users]
GO
ALTER TABLE [dbo].[Reportes] ADD  DEFAULT (getutcdate()) FOR [FechaGeneracion]
GO
ALTER TABLE [dbo].[Reportes] ADD  DEFAULT ('Completado') FOR [EstadoGeneracion]