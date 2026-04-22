CREATE TABLE [dbo].[EstadosReservaActividad]
(
    [EstadoReservaActividadId] INT           NOT NULL,
    [Nombre]                   NVARCHAR(50)  NOT NULL,
    [Descripcion]              NVARCHAR(200) NULL,
    CONSTRAINT [PK_EstadosReservaActividad] PRIMARY KEY CLUSTERED ([EstadoReservaActividadId] ASC)
)
