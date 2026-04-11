CREATE TABLE [dbo].[Reservas]
(
    [ReservaId]            INT            IDENTITY(1,1) NOT NULL,
    [HuespedId]            INT            NOT NULL,
    [HabitacionId]         INT            NULL,
    [NumeroReserva]        NVARCHAR(50)   NOT NULL,
    [FechaCheckIn]         DATETIME2(7)   NOT NULL,
    [FechaCheckOut]        DATETIME2(7)   NOT NULL,
    [NumeroHuespedes]      INT            NOT NULL CONSTRAINT [DF_Reservas_NumeroHuespedes]  DEFAULT (1),
    [NumeroNinos]          INT            NOT NULL CONSTRAINT [DF_Reservas_NumeroNinos]      DEFAULT (0),
    [MontoTotal]           DECIMAL(10, 2) NOT NULL,
    [MontoPagado]          DECIMAL(10, 2) NOT NULL CONSTRAINT [DF_Reservas_MontoPagado]     DEFAULT (0),
    [EstadoReservaId]      INT            NOT NULL CONSTRAINT [DF_Reservas_EstadoReservaId]  DEFAULT (1),
    [FechaCreacion]        DATETIME2(7)   NOT NULL CONSTRAINT [DF_Reservas_FechaCreacion]   DEFAULT (GETUTCDATE()),
    [FechaActualizacion]   DATETIME2(7)   NULL,
    [CheckInRealizado]     DATETIME2(7)   NULL,
    [CheckOutRealizado]    DATETIME2(7)   NULL,
    [CreadoPor]            NVARCHAR(450)  NULL,
    [ModificadoPor]        NVARCHAR(450)  NULL,
    [Observaciones]        NVARCHAR(1000) NULL,

    CONSTRAINT [PK_Reservas]               PRIMARY KEY CLUSTERED ([ReservaId] ASC),
    CONSTRAINT [UQ_Reservas_NumeroReserva]  UNIQUE NONCLUSTERED ([NumeroReserva] ASC),
    CONSTRAINT [CHK_Reservas_Fechas]        CHECK ([FechaCheckOut] > [FechaCheckIn]),
    CONSTRAINT [FK_Reservas_Habitaciones]   FOREIGN KEY ([HabitacionId])    REFERENCES [dbo].[Habitaciones]   ([HabitacionId]),
    CONSTRAINT [FK_Reservas_Huespedes]      FOREIGN KEY ([HuespedId])       REFERENCES [dbo].[Huespedes]      ([HuespedId]),
    CONSTRAINT [FK_Reservas_EstadosReserva] FOREIGN KEY ([EstadoReservaId]) REFERENCES [dbo].[EstadosReserva] ([EstadoReservaId])
)
GO

CREATE NONCLUSTERED INDEX [IX_Reservas_Estado_Fechas]   ON [dbo].[Reservas] ([EstadoReservaId] ASC, [FechaCheckIn] ASC, [FechaCheckOut] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Reservas_EstadoReservaId] ON [dbo].[Reservas] ([EstadoReservaId] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Reservas_HabitacionId]    ON [dbo].[Reservas] ([HabitacionId] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Reservas_HuespedId]       ON [dbo].[Reservas] ([HuespedId] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Reservas_NumeroReserva]   ON [dbo].[Reservas] ([NumeroReserva] ASC)
GO
