using Dispositivos.API.GrpcServices;
using Barcelo.Authorization.Shared;
using Dispositivos.Application;
using Dispositivos.Infrastructure;
using Dispositivos.Persistence;
using Dispositivos.Persistence.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;

namespace Dispositivos.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Habilitar HTTP/2 sin cifrado (h2c) para llamadas gRPC a Reservas.API
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var builder = WebApplication.CreateBuilder(args);

            // Enable HTTP/2 cleartext (h2c) for gRPC on the same port as REST
            if (!builder.Configuration.GetSection("Kestrel:Endpoints").Exists())
            {
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ConfigureEndpointDefaults(listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    });
                });
            }

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddGrpc();
            builder.Services.AddOpenApi();
            builder.Services.AddHttpContextAccessor();

            // Add Application Layer
            builder.Services.AddApplicationLayer();

            // Add Persistence Layer
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddPersistenceLayer(connectionString);

            // Add Thingsboard Infrastructure Layer
            builder.Services.AddThingsboardInfrastructure(builder.Configuration);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });
            builder.Services.AddBarceloAuthorization();

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
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapGrpcService<PersonalEstadoGrpcService>();
            app.Run();

           
        }
    }
}
