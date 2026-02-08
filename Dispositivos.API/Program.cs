using Dispositivos.Application;
using Dispositivos.Persistence;
using Scalar.AspNetCore;

namespace Dispositivos.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            // Add Application Layer
            builder.Services.AddApplicationLayer();

            // Add Persistence Layer
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddPersistenceLayer(connectionString);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
