CREATE TABLE [dbo].[PermisosPersonal](
	[PermisoId] [int] IDENTITY(1,1) NOT NULL,
	[PersonalId] [int] NOT NULL,
	[HabitacionId] [int] NULL,
	[TipoPermiso] [nvarchar](50) NOT NULL,
	[FechaOtorgamiento] [datetime2](7) NOT NULL,
	[FechaExpiracion] [datetime2](7) NULL,
	[EsTemporal] [bit] NOT NULL,
	[OtorgadoPor] [nvarchar](450) NOT NULL,
	[EstaActivo] [bit] NOT NULL,
	[Justificacion] [nvarchar](500) NULL,
 CONSTRAINT [PK_PermisosPersonal] PRIMARY KEY CLUSTERED 
(
	[PermisoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PermisosPersonal]  WITH CHECK ADD  CONSTRAINT [FK_PermisosPersonal_Habitaciones] FOREIGN KEY([HabitacionId])
REFERENCES [dbo].[Habitaciones] ([HabitacionId])
GO

ALTER TABLE [dbo].[PermisosPersonal] CHECK CONSTRAINT [FK_PermisosPersonal_Habitaciones]
GO
ALTER TABLE [dbo].[PermisosPersonal]  WITH CHECK ADD  CONSTRAINT [FK_PermisosPersonal_OtorgadoPor] FOREIGN KEY([OtorgadoPor])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[PermisosPersonal] CHECK CONSTRAINT [FK_PermisosPersonal_OtorgadoPor]
GO
ALTER TABLE [dbo].[PermisosPersonal]  WITH CHECK ADD  CONSTRAINT [FK_PermisosPersonal_Personal] FOREIGN KEY([PersonalId])
REFERENCES [dbo].[Personal] ([PersonalId])
GO

ALTER TABLE [dbo].[PermisosPersonal] CHECK CONSTRAINT [FK_PermisosPersonal_Personal]
GO
ALTER TABLE [dbo].[PermisosPersonal] ADD  DEFAULT (getutcdate()) FOR [FechaOtorgamiento]
GO
ALTER TABLE [dbo].[PermisosPersonal] ADD  DEFAULT ((0)) FOR [EsTemporal]
GO
ALTER TABLE [dbo].[PermisosPersonal] ADD  DEFAULT ((1)) FOR [EstaActivo]