CREATE TABLE [dbo].[Departamentos](
	[DepartamentoId] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[Descripcion] [nvarchar](500) NULL,
	[EstaActivo] [bit] NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[EliminadoEn] [datetime2](7) NULL,
 CONSTRAINT [PK_Departamentos] PRIMARY KEY CLUSTERED
(
	[DepartamentoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Departamentos_Nombre] UNIQUE NONCLUSTERED
(
	[Nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Departamentos] ADD  DEFAULT ((1)) FOR [EstaActivo]
GO
ALTER TABLE [dbo].[Departamentos] ADD  DEFAULT (getutcdate()) FOR [FechaCreacion]
GO
