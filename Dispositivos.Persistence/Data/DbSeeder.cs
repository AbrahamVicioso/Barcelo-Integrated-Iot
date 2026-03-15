using Dispositivos.Domain.Entities;
using Dispositivos.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Dispositivos.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(BarceloIoTDatabaseContext context)
    {
        await SeedTiposDispositivoAsync(context);
        await SeedEstadosDispositivoAsync(context);
    }

    private static async Task SeedTiposDispositivoAsync(BarceloIoTDatabaseContext context)
    {
        if (await context.TiposDispositivo.AnyAsync())
            return;

        var tipos = new List<TipoDispositivo>
        {
            new() { TipoDispositivoId = 1, Nombre = "Cerradura Inteligente" },
            new() { TipoDispositivoId = 2, Nombre = "Termostato" },
            new() { TipoDispositivoId = 3, Nombre = "Sensor de Movimiento" },
            new() { TipoDispositivoId = 4, Nombre = "Camara" },
            new() { TipoDispositivoId = 5, Nombre = "Control de Iluminacion" },
            new() { TipoDispositivoId = 6, Nombre = "Sensor de Temperatura" },
            new() { TipoDispositivoId = 7, Nombre = "Panel de Control" },
        };

        await context.TiposDispositivo.AddRangeAsync(tipos);
        await context.SaveChangesAsync();
    }

    private static async Task SeedEstadosDispositivoAsync(BarceloIoTDatabaseContext context)
    {
        if (await context.EstadosDispositivo.AnyAsync())
            return;

        var estados = new List<EstadoDispositivo>
        {
            new() { EstadoDispositivoId = 1, Descripcion = "Operativo" },
            new() { EstadoDispositivoId = 2, Descripcion = "Mantenimiento" },
            new() { EstadoDispositivoId = 3, Descripcion = "Falla" },
        };

        await context.EstadosDispositivo.AddRangeAsync(estados);
        await context.SaveChangesAsync();
    }
}
