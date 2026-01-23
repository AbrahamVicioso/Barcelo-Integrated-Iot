CREATE TABLE [dbo].[RegistrosAuditoria](
	[AuditoriaId] [bigint] IDENTITY(1,1) NOT NULL,
	[UsuarioId] [nvarchar](450) NULL,
	[Accion] [nvarchar](200) NOT NULL,
	[TipoEntidad] [nvarchar](100) NOT NULL,
	[EntidadId] [int] NULL,
	[ValorAnterior] [nvarchar](max) NULL,
	[ValorNuevo] [nvarchar](max) NULL,
	[FechaHora] [datetime2](7) NOT NULL,
	[DireccionIP] [nvarchar](50) NULL,
	[AgenteUsuario] [nvarchar](500) NULL,
	[Resultado] [nvarchar](50) NOT NULL,
	[MensajeError] [nvarchar](1000) NULL,
	[HotelId] [int] NULL,
 CONSTRAINT [PK_RegistrosAuditoria] PRIMARY KEY CLUSTERED 
(
	[AuditoriaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[RegistrosAuditoria]  WITH CHECK ADD  CONSTRAINT [FK_Auditoria_Hoteles] FOREIGN KEY([HotelId])
REFERENCES [dbo].[Hoteles] ([HotelId])
GO

ALTER TABLE [dbo].[RegistrosAuditoria] CHECK CONSTRAINT [FK_Auditoria_Hoteles]
GO
ALTER TABLE [dbo].[RegistrosAuditoria]  WITH CHECK ADD  CONSTRAINT [FK_Auditoria_Users] FOREIGN KEY([UsuarioId])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[RegistrosAuditoria] CHECK CONSTRAINT [FK_Auditoria_Users]
GO
ALTER TABLE [dbo].[RegistrosAuditoria] ADD  DEFAULT (getutcdate()) FOR [FechaHora]
GO
/****** Object:  Index [IX_Auditoria_TipoEntidad]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Auditoria_TipoEntidad] ON [dbo].[RegistrosAuditoria]
(
	[TipoEntidad] ASC,
	[EntidadId] ASC,
	[FechaHora] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Auditoria_UsuarioId_Fecha]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Auditoria_UsuarioId_Fecha] ON [dbo].[RegistrosAuditoria]
(
	[UsuarioId] ASC,
	[FechaHora] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]