CREATE TABLE [Seguridad].[ClavesEncriptacion](
	[ClaveId] [int] IDENTITY(1,1) NOT NULL,
	[NombreClave] [nvarchar](100) NOT NULL,
	[ClaveEncriptada] [varbinary](max) NOT NULL,
	[Algoritmo] [nvarchar](50) NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[FechaExpiracion] [datetime2](7) NULL,
	[EstaActiva] [bit] NOT NULL,
 CONSTRAINT [PK_ClavesEncriptacion] PRIMARY KEY CLUSTERED 
(
	[ClaveId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [Seguridad].[ClavesEncriptacion] ADD  DEFAULT (getutcdate()) FOR [FechaCreacion]
GO
ALTER TABLE [Seguridad].[ClavesEncriptacion] ADD  DEFAULT ((1)) FOR [EstaActiva]