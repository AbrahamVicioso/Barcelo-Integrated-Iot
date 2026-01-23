CREATE TABLE [dbo].[Huespedes](
	[HuespedId] [int] IDENTITY(1,1) NOT NULL,
	[UsuarioId] [nvarchar](450) NOT NULL,
	[NombreCompleto] [nvarchar](200) NOT NULL,
	[TipoDocumento] [nvarchar](50) NOT NULL,
	[NumeroDocumento] [nvarchar](100) NOT NULL,
	[Nacionalidad] [nvarchar](100) NOT NULL,
	[FechaNacimiento] [date] NOT NULL,
	[ContactoEmergencia] [nvarchar](200) NULL,
	[TelefonoEmergencia] [nvarchar](50) NULL,
	[EsVIP] [bit] NOT NULL,
	[FechaRegistro] [datetime2](7) NOT NULL,
	[PreferenciasAlimentarias] [nvarchar](500) NULL,
	[NotasEspeciales] [nvarchar](1000) NULL,
 CONSTRAINT [PK_Huespedes] PRIMARY KEY CLUSTERED 
(
	[HuespedId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Huespedes_Documento] UNIQUE NONCLUSTERED 
(
	[TipoDocumento] ASC,
	[NumeroDocumento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Huespedes_Usuario] UNIQUE NONCLUSTERED 
(
	[UsuarioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Huespedes]  WITH CHECK ADD  CONSTRAINT [FK_Huespedes_Users] FOREIGN KEY([UsuarioId])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[Huespedes] CHECK CONSTRAINT [FK_Huespedes_Users]
GO
ALTER TABLE [dbo].[Huespedes] ADD  DEFAULT ((0)) FOR [EsVIP]
GO
ALTER TABLE [dbo].[Huespedes] ADD  DEFAULT (getutcdate()) FOR [FechaRegistro]