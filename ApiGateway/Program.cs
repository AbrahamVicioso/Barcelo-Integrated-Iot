using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Threading.Tasks;

namespace ApiGateway
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
            builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

            builder.Services.AddOcelot();

            var allowedOrigins = builder.Configuration
                .GetSection("AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:3000"];

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });

            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
            {
                app.MapOpenApi();
            }

            app.UseWebSockets();

            app.UseAuthorization();

            app.UseCors();

            // Propagate the gateway's public address to downstream services via forwarded headers.
            // This allows services to build correct public-facing URLs (e.g. email confirmation links)
            // regardless of where they're deployed.
            app.Use(async (ctx, next) =>
            {
                ctx.Request.Headers["X-Forwarded-Proto"] = ctx.Request.Scheme;
                ctx.Request.Headers["X-Forwarded-Host"] = ctx.Request.Host.Value;
                await next(ctx);
            });

            await app.UseOcelot();

            app.MapControllers();

            app.Run();
        }
    }
}
