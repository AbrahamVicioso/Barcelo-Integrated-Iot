CREATE TABLE [dbo].[EstadosReserva]
(
    [EstadoReservaId] INT           NOT NULL,
    [Nombre]          NVARCHAR(50)  NOT NULL,
    [Descripcion]     NVARCHAR(200) NULL,
    CONSTRAINT [PK_EstadosReserva] PRIMARY KEY CLUSTERED ([EstadoReservaId] ASC)
)
