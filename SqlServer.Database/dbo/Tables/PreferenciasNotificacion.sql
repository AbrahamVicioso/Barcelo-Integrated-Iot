CREATE TABLE [dbo].[PreferenciasNotificacion](
	[PreferenciaId] [int] IDENTITY(1,1) NOT NULL,
	[UsuarioId] [nvarchar](450) NOT NULL,
	[HabilitarNotificacionesPush] [bit] NOT NULL,
	[HabilitarNotificacionesEmail] [bit] NOT NULL,
	[HabilitarNotificacionesSMS] [bit] NOT NULL,
	[NotificarAccesoPersonal] [bit] NOT NULL,
	[NotificarRecordatorioActividad] [bit] NOT NULL,
	[NotificarPromocionesOfertas] [bit] NOT NULL,
	[HorarioNoMolestar] [bit] NOT NULL,
	[HoraInicioNoMolestar] [time](7) NULL,
	[HoraFinNoMolestar] [time](7) NULL,
	[FechaActualizacion] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_PreferenciasNotificacion] PRIMARY KEY CLUSTERED 
(
	[PreferenciaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_PreferenciasNotif_Usuario] UNIQUE NONCLUSTERED 
(
	[UsuarioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PreferenciasNotificacion]  WITH CHECK ADD  CONSTRAINT [FK_PreferenciasNotif_Users] FOREIGN KEY([UsuarioId])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[PreferenciasNotificacion] CHECK CONSTRAINT [FK_PreferenciasNotif_Users]
GO
ALTER TABLE [dbo].[PreferenciasNotificacion] ADD  DEFAULT ((1)) FOR [HabilitarNotificacionesPush]
GO
ALTER TABLE [dbo].[PreferenciasNotificacion] ADD  DEFAULT ((1)) FOR [HabilitarNotificacionesEmail]
GO
ALTER TABLE [dbo].[PreferenciasNotificacion] ADD  DEFAULT ((0)) FOR [HabilitarNotificacionesSMS]
GO
ALTER TABLE [dbo].[PreferenciasNotificacion] ADD  DEFAULT ((1)) FOR [NotificarAccesoPersonal]
GO
ALTER TABLE [dbo].[PreferenciasNotificacion] ADD  DEFAULT ((1)) FOR [NotificarRecordatorioActividad]
GO
ALTER TABLE [dbo].[PreferenciasNotificacion] ADD  DEFAULT ((1)) FOR [NotificarPromocionesOfertas]
GO
ALTER TABLE [dbo].[PreferenciasNotificacion] ADD  DEFAULT ((0)) FOR [HorarioNoMolestar]
GO
ALTER TABLE [dbo].[PreferenciasNotificacion] ADD  DEFAULT (getutcdate()) FOR [FechaActualizacion]