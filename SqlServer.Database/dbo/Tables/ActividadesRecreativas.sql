CREATE TABLE [dbo].[ActividadesRecreativas](
	[ActividadId] [int] IDENTITY(1,1) NOT NULL,
	[HotelId] [int] NOT NULL,
	[NombreActividad] [nvarchar](200) NOT NULL,
	[Descripcion] [nvarchar](2000) NULL,
	[Categoria] [nvarchar](50) NOT NULL,
	[Ubicacion] [nvarchar](200) NOT NULL,
	[HoraApertura] [time](7) NOT NULL,
	[HoraCierre] [time](7) NOT NULL,
	[CapacidadMaxima] [int] NOT NULL,
	[PrecioPorPersona] [decimal](10, 2) NOT NULL,
	[RequiereReserva] [bit] NOT NULL,
	[DuracionMinutos] [int] NULL,
	[EstaActiva] [bit] NOT NULL,
	[ImagenURL] [nvarchar](500) NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_ActividadesRecreativas] PRIMARY KEY CLUSTERED 
(
	[ActividadId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ActividadesRecreativas]  WITH CHECK ADD  CONSTRAINT [FK_Actividades_Hoteles] FOREIGN KEY([HotelId])
REFERENCES [dbo].[Hoteles] ([HotelId])
GO

ALTER TABLE [dbo].[ActividadesRecreativas] CHECK CONSTRAINT [FK_Actividades_Hoteles]
GO
ALTER TABLE [dbo].[ActividadesRecreativas] ADD  DEFAULT ((0)) FOR [PrecioPorPersona]
GO
ALTER TABLE [dbo].[ActividadesRecreativas] ADD  DEFAULT ((0)) FOR [RequiereReserva]
GO
ALTER TABLE [dbo].[ActividadesRecreativas] ADD  DEFAULT ((1)) FOR [EstaActiva]
GO
ALTER TABLE [dbo].[ActividadesRecreativas] ADD  DEFAULT (getutcdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[ActividadesRecreativas]  WITH CHECK ADD  CONSTRAINT [CHK_Actividades_Horario] CHECK  (([HoraCierre]>[HoraApertura]))
GO

ALTER TABLE [dbo].[ActividadesRecreativas] CHECK CONSTRAINT [CHK_Actividades_Horario]