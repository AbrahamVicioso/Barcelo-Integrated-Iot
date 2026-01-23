CREATE TABLE [dbo].[CredencialesAcceso](
	[CredencialId] [int] IDENTITY(1,1) NOT NULL,
	[HuespedId] [int] NULL,
	[PersonalId] [int] NULL,
	[CodigoPIN] [nvarchar](6) NOT NULL,
	[HashPIN] [nvarchar](256) NOT NULL,
	[FechaActivacion] [datetime2](7) NOT NULL,
	[FechaExpiracion] [datetime2](7) NOT NULL,
	[EstaActiva] [bit] NOT NULL,
	[TipoCredencial] [nvarchar](30) NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[CreadoPor] [nvarchar](450) NULL,
	[NumeroUsos] [int] NOT NULL,
	[UltimoUso] [datetime2](7) NULL,
 CONSTRAINT [PK_CredencialesAcceso] PRIMARY KEY CLUSTERED 
(
	[CredencialId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CredencialesAcceso]  WITH CHECK ADD  CONSTRAINT [FK_Credenciales_Huespedes] FOREIGN KEY([HuespedId])
REFERENCES [dbo].[Huespedes] ([HuespedId])
GO

ALTER TABLE [dbo].[CredencialesAcceso] CHECK CONSTRAINT [FK_Credenciales_Huespedes]
GO
ALTER TABLE [dbo].[CredencialesAcceso]  WITH CHECK ADD  CONSTRAINT [FK_Credenciales_Personal] FOREIGN KEY([PersonalId])
REFERENCES [dbo].[Personal] ([PersonalId])
GO

ALTER TABLE [dbo].[CredencialesAcceso] CHECK CONSTRAINT [FK_Credenciales_Personal]
GO
ALTER TABLE [dbo].[CredencialesAcceso] ADD  DEFAULT ((1)) FOR [EstaActiva]
GO
ALTER TABLE [dbo].[CredencialesAcceso] ADD  DEFAULT (getutcdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[CredencialesAcceso] ADD  DEFAULT ((0)) FOR [NumeroUsos]
GO
ALTER TABLE [dbo].[CredencialesAcceso]  WITH CHECK ADD  CONSTRAINT [CHK_Credenciales_Fechas] CHECK  (([FechaExpiracion]>[FechaActivacion]))
GO

ALTER TABLE [dbo].[CredencialesAcceso] CHECK CONSTRAINT [CHK_Credenciales_Fechas]
GO
ALTER TABLE [dbo].[CredencialesAcceso]  WITH CHECK ADD  CONSTRAINT [CHK_Credenciales_Relacion] CHECK  (([HuespedId] IS NOT NULL AND [PersonalId] IS NULL OR [HuespedId] IS NULL AND [PersonalId] IS NOT NULL OR [HuespedId] IS NULL AND [PersonalId] IS NULL AND [TipoCredencial]='Temporal'))
GO

ALTER TABLE [dbo].[CredencialesAcceso] CHECK CONSTRAINT [CHK_Credenciales_Relacion]
GO
/****** Object:  Index [IX_Credenciales_Activas]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Credenciales_Activas] ON [dbo].[CredencialesAcceso]
(
	[EstaActiva] ASC,
	[FechaExpiracion] ASC
)
WHERE ([EstaActiva]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Credenciales_HuespedId]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Credenciales_HuespedId] ON [dbo].[CredencialesAcceso]
(
	[HuespedId] ASC
)
WHERE ([HuespedId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Credenciales_PersonalId]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_Credenciales_PersonalId] ON [dbo].[CredencialesAcceso]
(
	[PersonalId] ASC
)
WHERE ([PersonalId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]