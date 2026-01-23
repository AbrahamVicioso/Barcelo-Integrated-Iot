CREATE TABLE [dbo].[ConfiguracionSistema](
	[ConfiguracionId] [int] IDENTITY(1,1) NOT NULL,
	[HotelId] [int] NULL,
	[Clave] [nvarchar](100) NOT NULL,
	[Valor] [nvarchar](max) NOT NULL,
	[Descripcion] [nvarchar](500) NULL,
	[TipoDato] [nvarchar](50) NOT NULL,
	[EsGlobal] [bit] NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[FechaActualizacion] [datetime2](7) NULL,
	[ModificadoPor] [nvarchar](450) NULL,
 CONSTRAINT [PK_ConfiguracionSistema] PRIMARY KEY CLUSTERED 
(
	[ConfiguracionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Configuracion_Clave] UNIQUE NONCLUSTERED 
(
	[HotelId] ASC,
	[Clave] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[ConfiguracionSistema]  WITH CHECK ADD  CONSTRAINT [FK_Configuracion_Hoteles] FOREIGN KEY([HotelId])
REFERENCES [dbo].[Hoteles] ([HotelId])
GO

ALTER TABLE [dbo].[ConfiguracionSistema] CHECK CONSTRAINT [FK_Configuracion_Hoteles]
GO
ALTER TABLE [dbo].[ConfiguracionSistema] ADD  DEFAULT ((0)) FOR [EsGlobal]
GO
ALTER TABLE [dbo].[ConfiguracionSistema] ADD  DEFAULT (getutcdate()) FOR [FechaCreacion]