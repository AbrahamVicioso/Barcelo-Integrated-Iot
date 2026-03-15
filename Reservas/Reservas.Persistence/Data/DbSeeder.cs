using Microsoft.EntityFrameworkCore;
using Reservas.Domain.Entities;
using Reservas.Persistence.Data;

namespace Reservas.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(BarceloReservasContext context)
    {
        await SeedEstadosHabitacionAsync(context);
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
