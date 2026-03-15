using Microsoft.EntityFrameworkCore;
using Reservas.Domain.Entities;
using Reservas.Persistence.Data;

namespace Reservas.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(BarceloReservasContext context)
    {
        await SeedTiposHabitacionAsync(context);
        await SeedEstadosHabitacionAsync(context);
    }

    private static async Task SeedTiposHabitacionAsync(BarceloReservasContext context)
    {
        if (await context.TiposHabitacion.AnyAsync())
            return;

        var tipos = new List<TipoHabitacion>
        {
            new() { TipoHabitacionId = 1, Nombre = "Simple" },
            new() { TipoHabitacionId = 2, Nombre = "Doble" },
            new() { TipoHabitacionId = 3, Nombre = "Triple" },
            new() { TipoHabitacionId = 4, Nombre = "Suite" },
            new() { TipoHabitacionId = 5, Nombre = "Suite Junior" },
            new() { TipoHabitacionId = 6, Nombre = "Suite Presidencial" },
            new() { TipoHabitacionId = 7, Nombre = "Familiar" },
        };

        await context.TiposHabitacion.AddRangeAsync(tipos);
        await context.SaveChangesAsync();
    }

    private static async Task SeedEstadosHabitacionAsync(BarceloReservasContext context)
    {
        if (await context.EstadosHabitacion.AnyAsync())
            return;

        var estados = new List<EstadoHabitacion>
        {
            new() { EstadoHabitacionId = 1, Descripcion = "Disponible" },
            new() { EstadoHabitacionId = 2, Descripcion = "Ocupada" },
            new() { EstadoHabitacionId = 3, Descripcion = "Mantenimiento" },
            new() { EstadoHabitacionId = 4, Descripcion = "Fuera de Servicio" },
            new() { EstadoHabitacionId = 5, Descripcion = "Limpieza" },
        };

        await context.EstadosHabitacion.AddRangeAsync(estados);
        await context.SaveChangesAsync();
    }
}
