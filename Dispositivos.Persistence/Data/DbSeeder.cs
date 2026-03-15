using Dispositivos.Domain.Entities;
using Dispositivos.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Dispositivos.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(BarceloIoTDatabaseContext context)
    {
        await SeedEstadosDispositivoAsync(context);
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
