CREATE TABLE [dbo].[TiposHabitacion] (
    [TipoHabitacionId] INT          NOT NULL,
    [Nombre]           NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_TiposHabitacion] PRIMARY KEY CLUSTERED ([TipoHabitacionId] ASC)
)
