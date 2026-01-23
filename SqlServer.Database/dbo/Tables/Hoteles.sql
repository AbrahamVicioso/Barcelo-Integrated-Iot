CREATE TABLE [dbo].[Hoteles](
	[HotelId] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](200) NOT NULL,
	[Direccion] [nvarchar](500) NOT NULL,
	[Ciudad] [nvarchar](100) NOT NULL,
	[Pais] [nvarchar](100) NOT NULL,
	[Telefono] [nvarchar](50) NULL,
	[Email] [nvarchar](256) NULL,
	[EstaActivo] [bit] NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[NumeroHabitaciones] [int] NOT NULL,
	[NumeroEstrellas] [tinyint] NULL,
 CONSTRAINT [PK_Hoteles] PRIMARY KEY CLUSTERED 
(
	[HotelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Hoteles] ADD  DEFAULT ((1)) FOR [EstaActivo]
GO
ALTER TABLE [dbo].[Hoteles] ADD  DEFAULT (getutcdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Hoteles] ADD  DEFAULT ((0)) FOR [NumeroHabitaciones]
GO
ALTER TABLE [dbo].[Hoteles]  WITH CHECK ADD CHECK  (([NumeroEstrellas]>=(1) AND [NumeroEstrellas]<=(5)))