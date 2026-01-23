CREATE TABLE [dbo].[LogsSistema](
	[LogId] [bigint] IDENTITY(1,1) NOT NULL,
	[Nivel] [nvarchar](20) NOT NULL,
	[Mensaje] [nvarchar](max) NOT NULL,
	[Excepcion] [nvarchar](max) NULL,
	[StackTrace] [nvarchar](max) NULL,
	[Fuente] [nvarchar](200) NOT NULL,
	[UsuarioId] [nvarchar](450) NULL,
	[FechaHora] [datetime2](7) NOT NULL,
	[DireccionIP] [nvarchar](50) NULL,
	[Metadata] [nvarchar](max) NULL,
 CONSTRAINT [PK_LogsSistema] PRIMARY KEY CLUSTERED 
(
	[LogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[LogsSistema] ADD  DEFAULT (getutcdate()) FOR [FechaHora]