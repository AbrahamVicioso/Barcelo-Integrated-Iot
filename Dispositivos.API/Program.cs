using Dispositivos.Application;
using Dispositivos.Infrastructure;
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

            // Add Thingsboard Infrastructure Layer
            builder.Services.AddThingsboardInfrastructure(builder.Configuration);

            builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5019")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

           // app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowFrontend");
            app.UseAuthorization();
            app.MapControllers();
            app.Run();

           
        }
    }
}
