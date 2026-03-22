CREATE TABLE [dbo].[EstadosReserva]
(
	[EstadoReservaId] INT NOT NULL,
	[Nombre]          NVARCHAR(50)  NOT NULL,
	[Descripcion]     NVARCHAR(200) NULL,
	CONSTRAINT [PK_EstadosReserva] PRIMARY KEY CLUSTERED ([EstadoReservaId] ASC)
)
GO

INSERT INTO [dbo].[EstadosReserva] ([EstadoReservaId], [Nombre], [Descripcion]) VALUES
(1, N'Pendiente', N'Reserva creada, pendiente de check-in'),
(2, N'Activa',    N'Check-in realizado, huésped en el hotel'),
(3, N'CheckOut',  N'Check-out realizado, reserva finalizada'),
(4, N'Cancelada', N'Reserva cancelada')
GO
