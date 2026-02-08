CREATE TABLE [dbo].[MantenimientoCerraduras](
	[MantenimientoId] [int] IDENTITY(1,1) NOT NULL,
	[DispositivoId] UNIQUEIDENTIFIER NULL,
	[CerraduraId] [int] NULL,
	[TipoMantenimiento] [nvarchar](50) NOT NULL,
	[FechaProgramada] [datetime2](7) NOT NULL,
	[FechaRealizada] [datetime2](7) NULL,
	[PersonalId] [int] NULL,
	[Estado] [nvarchar](30) NOT NULL,
	[Observaciones] [nvarchar](1000) NULL,
	[CostoMantenimiento] [decimal](10, 2) NULL,
	[TiempoEmpleadoMinutos] [int] NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_MantenimientoCerraduras] PRIMARY KEY CLUSTERED 
(
	[MantenimientoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MantenimientoCerraduras]  WITH CHECK ADD  CONSTRAINT [FK_Mantenimiento_Cerraduras] FOREIGN KEY([CerraduraId])
REFERENCES [dbo].[CerradurasInteligentes] ([CerraduraId])
GO

ALTER TABLE [dbo].[MantenimientoCerraduras] CHECK CONSTRAINT [FK_Mantenimiento_Cerraduras]
GO
ALTER TABLE [dbo].[MantenimientoCerraduras]  WITH CHECK ADD  CONSTRAINT [FK_Mantenimiento_Dispositivos] FOREIGN KEY([DispositivoId])
REFERENCES [dbo].[Dispositivos] ([DispositivoId])
GO

ALTER TABLE [dbo].[MantenimientoCerraduras] CHECK CONSTRAINT [FK_Mantenimiento_Dispositivos]
GO
ALTER TABLE [dbo].[MantenimientoCerraduras]  WITH CHECK ADD  CONSTRAINT [FK_Mantenimiento_Personal] FOREIGN KEY([PersonalId])
REFERENCES [dbo].[Personal] ([PersonalId])
GO

ALTER TABLE [dbo].[MantenimientoCerraduras] CHECK CONSTRAINT [FK_Mantenimiento_Personal]
GO
ALTER TABLE [dbo].[MantenimientoCerraduras] ADD  DEFAULT ('Programado') FOR [Estado]
GO
ALTER TABLE [dbo].[MantenimientoCerraduras] ADD  DEFAULT (getutcdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[MantenimientoCerraduras]  WITH CHECK ADD  CONSTRAINT [CHK_Mantenimiento_Entidad] CHECK  (([DispositivoId] IS NOT NULL AND [CerraduraId] IS NULL OR [DispositivoId] IS NULL AND [CerraduraId] IS NOT NULL))
GO

ALTER TABLE [dbo].[MantenimientoCerraduras] CHECK CONSTRAINT [CHK_Mantenimiento_Entidad]