CREATE TABLE [dbo].[Personal](
	[PersonalId] [int] IDENTITY(1,1) NOT NULL,
	[UsuarioId] [nvarchar](450) NOT NULL,
	[HotelId] [int] NOT NULL,
	[NombreCompleto] [nvarchar](200) NOT NULL,
	[Puesto] [nvarchar](100) NOT NULL,
	[Departamento] [nvarchar](100) NOT NULL,
	[NumeroEmpleado] [nvarchar](50) NOT NULL,
	[FechaContratacion] [date] NOT NULL,
	[EstaActivo] [bit] NOT NULL,
	[Turno] [nvarchar](20) NULL,
	[Supervisor] [int] NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Personal] PRIMARY KEY CLUSTERED 
(
	[PersonalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Personal_NumeroEmpleado] UNIQUE NONCLUSTERED 
(
	[NumeroEmpleado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Personal_Usuario] UNIQUE NONCLUSTERED 
(
	[UsuarioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Personal]  WITH CHECK ADD  CONSTRAINT [FK_Personal_Hoteles] FOREIGN KEY([HotelId])
REFERENCES [dbo].[Hoteles] ([HotelId])
GO

ALTER TABLE [dbo].[Personal] CHECK CONSTRAINT [FK_Personal_Hoteles]
GO
ALTER TABLE [dbo].[Personal]  WITH CHECK ADD  CONSTRAINT [FK_Personal_Supervisor] FOREIGN KEY([Supervisor])
REFERENCES [dbo].[Personal] ([PersonalId])
GO

ALTER TABLE [dbo].[Personal] CHECK CONSTRAINT [FK_Personal_Supervisor]
GO
ALTER TABLE [dbo].[Personal]  WITH CHECK ADD  CONSTRAINT [FK_Personal_Users] FOREIGN KEY([UsuarioId])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[Personal] CHECK CONSTRAINT [FK_Personal_Users]
GO
ALTER TABLE [dbo].[Personal] ADD  DEFAULT ((1)) FOR [EstaActivo]
GO
ALTER TABLE [dbo].[Personal] ADD  DEFAULT (getutcdate()) FOR [FechaCreacion]