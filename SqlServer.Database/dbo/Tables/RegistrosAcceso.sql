CREATE TABLE [dbo].[RegistrosAcceso](
	[RegistroId] [bigint] IDENTITY(1,1) NOT NULL,
	[CerraduraId] [int] NOT NULL,
	[CredencialId] [int] NULL,
	[UsuarioId] [nvarchar](450) NULL,
	[FechaHoraAcceso] [datetime2](7) NOT NULL,
	[TipoAcceso] [nvarchar](50) NOT NULL,
	[ResultadoAcceso] [nvarchar](20) NOT NULL,
	[MotivoAcceso] [nvarchar](200) NULL,
	[DireccionIP] [nvarchar](50) NULL,
	[InfoDispositivo] [nvarchar](500) NULL,
	[FueExitoso] [bit] NOT NULL,
	[CodigoError] [nvarchar](50) NULL,
	[Latencia] [int] NULL,
 CONSTRAINT [PK_RegistrosAcceso] PRIMARY KEY CLUSTERED 
(
	[RegistroId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[RegistrosAcceso]  WITH CHECK ADD  CONSTRAINT [FK_RegistrosAcceso_Cerraduras] FOREIGN KEY([CerraduraId])
REFERENCES [dbo].[CerradurasInteligentes] ([CerraduraId])
GO

ALTER TABLE [dbo].[RegistrosAcceso] CHECK CONSTRAINT [FK_RegistrosAcceso_Cerraduras]
GO
ALTER TABLE [dbo].[RegistrosAcceso]  WITH CHECK ADD  CONSTRAINT [FK_RegistrosAcceso_Credenciales] FOREIGN KEY([CredencialId])
REFERENCES [dbo].[CredencialesAcceso] ([CredencialId])
GO

ALTER TABLE [dbo].[RegistrosAcceso] CHECK CONSTRAINT [FK_RegistrosAcceso_Credenciales]
GO
ALTER TABLE [dbo].[RegistrosAcceso] ADD  DEFAULT (getutcdate()) FOR [FechaHoraAcceso]
GO
/****** Object:  Index [IX_RegistrosAcceso_CerraduraId_Fecha]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_RegistrosAcceso_CerraduraId_Fecha] ON [dbo].[RegistrosAcceso]
(
	[CerraduraId] ASC,
	[FechaHoraAcceso] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RegistrosAcceso_TipoAcceso]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_RegistrosAcceso_TipoAcceso] ON [dbo].[RegistrosAcceso]
(
	[TipoAcceso] ASC,
	[FechaHoraAcceso] DESC
)
INCLUDE([ResultadoAcceso],[FueExitoso]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RegistrosAcceso_UsuarioId_Fecha]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_RegistrosAcceso_UsuarioId_Fecha] ON [dbo].[RegistrosAcceso]
(
	[UsuarioId] ASC,
	[FechaHoraAcceso] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]