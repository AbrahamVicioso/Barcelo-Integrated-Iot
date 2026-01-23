CREATE TABLE [dbo].[Notificaciones](
	[NotificacionId] [bigint] IDENTITY(1,1) NOT NULL,
	[UsuarioId] [nvarchar](450) NOT NULL,
	[TipoNotificacion] [nvarchar](50) NOT NULL,
	[Titulo] [nvarchar](200) NOT NULL,
	[Mensaje] [nvarchar](1000) NOT NULL,
	[Prioridad] [nvarchar](20) NOT NULL,
	[FueLeida] [bit] NOT NULL,
	[FechaEnvio] [datetime2](7) NOT NULL,
	[FechaLectura] [datetime2](7) NULL,
	[TipoEntidadRelacionada] [nvarchar](100) NULL,
	[EntidadRelacionadaId] [int] NULL,
	[CanalEnvio] [nvarchar](50) NOT NULL,
	[EstadoEnvio] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_Notificaciones] PRIMARY KEY CLUSTERED 
(
	[NotificacionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Notificaciones]  WITH CHECK ADD  CONSTRAINT [FK_Notificaciones_Users] FOREIGN KEY([UsuarioId])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[Notificaciones] CHECK CONSTRAINT [FK_Notificaciones_Users]
GO
ALTER TABLE [dbo].[Notificaciones] ADD  DEFAULT ('Media') FOR [Prioridad]
GO
ALTER TABLE [dbo].[Notificaciones] ADD  DEFAULT ((0)) FOR [FueLeida]
GO
ALTER TABLE [dbo].[Notificaciones] ADD  DEFAULT (getutcdate()) FOR [FechaEnvio]
GO
ALTER TABLE [dbo].[Notificaciones] ADD  DEFAULT ('Push') FOR [CanalEnvio]
GO
ALTER TABLE [dbo].[Notificaciones] ADD  DEFAULT ('Enviada') FOR [EstadoEnvio]
GO
/****** Object:  Index [IX_Notificaciones_NoLeidas]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Notificaciones_NoLeidas] ON [dbo].[Notificaciones]
(
	[UsuarioId] ASC,
	[FueLeida] ASC
)
WHERE ([FueLeida]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Notificaciones_UsuarioId_FechaEnvio]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Notificaciones_UsuarioId_FechaEnvio] ON [dbo].[Notificaciones]
(
	[UsuarioId] ASC,
	[FechaEnvio] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]