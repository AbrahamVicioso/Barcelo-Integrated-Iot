CREATE TABLE [dbo].[ReservaHuespedes]
(
    [ReservaId]                        INT          NOT NULL,
    [HuespedId]                        INT          NOT NULL,
    [PuedeCrearActividadesRecreativas]  BIT          NOT NULL CONSTRAINT [DF_ReservaHuespedes_PuedeCrearActividades] DEFAULT (0),
    [PuedeDesbloquearCerradura]         BIT          NOT NULL CONSTRAINT [DF_ReservaHuespedes_PuedeDesbloquearCerradura] DEFAULT (0),
    [FechaAgregado]                    DATETIME2(7) NOT NULL CONSTRAINT [DF_ReservaHuespedes_FechaAgregado] DEFAULT (GETUTCDATE()),

    CONSTRAINT [PK_ReservaHuespedes]          PRIMARY KEY CLUSTERED ([ReservaId] ASC, [HuespedId] ASC),
    CONSTRAINT [FK_ReservaHuespedes_Reservas]  FOREIGN KEY ([ReservaId]) REFERENCES [dbo].[Reservas]  ([ReservaId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ReservaHuespedes_Huespedes] FOREIGN KEY ([HuespedId]) REFERENCES [dbo].[Huespedes] ([HuespedId])
)
GO

CREATE NONCLUSTERED INDEX [IX_ReservaHuespedes_HuespedId] ON [dbo].[ReservaHuespedes] ([HuespedId] ASC)
GO
