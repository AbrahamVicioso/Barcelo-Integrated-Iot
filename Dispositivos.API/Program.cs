using Dispositivos.Application;
using Dispositivos.Infrastructure;
using Dispositivos.Persistence;
using Dispositivos.Persistence.Data;
using Scalar.AspNetCore;

namespace Dispositivos.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddHttpContextAccessor();

            // Add Application Layer
            builder.Services.AddApplicationLayer();

            // Add Persistence Layer
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddPersistenceLayer(connectionString);

            // Add Thingsboard Infrastructure Layer
            builder.Services.AddThingsboardInfrastructure(builder.Configuration);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BarceloIoTDatabaseContext>();
                await context.Database.EnsureCreatedAsync();
                await DbSeeder.SeedAsync(context);
            }

            app.UseCors("AllowAll");
            app.UseAuthorization();
            app.MapControllers();
            app.Run();

           
        }
    }
}
