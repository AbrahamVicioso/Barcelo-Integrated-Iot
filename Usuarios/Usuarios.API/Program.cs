using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Usuarios.API.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Notification.Domain.Interfaces;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;
using Usuarios.Application.Behaviors;
using Usuarios.Application.Mappings;
using Usuarios.API.GrpcServices;
using Usuarios.API.Services;
using Usuarios.Domain.Interfaces;
using Usuarios.Persistence.Data;
using Usuarios.Persistence.Repositories;
using Usuarios.Application;
using Usuarios.ExternalService;

namespace Usuarios.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Habilitar HTTP/2 sin cifrado (h2c) para clientes gRPC internos
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var builder = WebApplication.CreateBuilder(args);

            // Si hay endpoints específicos en appsettings (ej: Docker con HTTPS gRPC en 5285),
            // Kestrel los usa directamente. En dev local se usa h2c en todos los endpoints.
            if (!builder.Configuration.GetSection("Kestrel:Endpoints").Exists())
            {
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ConfigureEndpointDefaults(listenOptions =>
                    {
                        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                    });
                });
            }

            // Add DbContext
            builder.Services.AddDbContext<BarceloIoTSystemContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddApplicationService();
            builder.Services.AddExternalServices(builder.Configuration);

            // Add MediatR with Audit pipeline behavior
            builder.Services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(Usuarios.Application.UseCases.Huespedes.Commands.CreateHuespede.CreateHuespedeCommand).Assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
            });

            // Register Audit Kafka Producer
            var auditConfig = new AuditKafkaProducerConfig();
            builder.Configuration.GetSection("AuditProducer").Bind(auditConfig);
            builder.Services.AddSingleton(auditConfig);
            builder.Services.AddSingleton<IAuditProducer, AuditKafkaProducer>();

            // Register Permiso Habitacion Kafka Producer (notifies Dispositivos to sync ThingsBoard)
            var permisoProducerConfig = new PermisoHabitacionKafkaProducerConfig();
            builder.Configuration.GetSection("KafkaProducer:PermisoHabitacion").Bind(permisoProducerConfig);
            builder.Services.AddSingleton(permisoProducerConfig);
            builder.Services.AddSingleton<Usuarios.Application.Interfaces.IPermisoHabitacionSyncProducer, PermisoHabitacionKafkaProducer>();

            // Required for AuditBehavior
            builder.Services.AddHttpContextAccessor();

            // Add FluentValidation
            builder.Services.AddValidatorsFromAssembly(typeof(Usuarios.Application.Validators.Huespedes.CreateHuespedeDtoValidator).Assembly);

            // Add Repositories and Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IHuespedeRepository, HuespedeRepository>();
            builder.Services.AddScoped<IPersonalRepository, PersonalRepository>();
            builder.Services.AddScoped<IPermisosPersonalRepository, PermisosPersonalRepository>();

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
            builder.Services.AddAuthorization();

            // Add Controllers + gRPC
            builder.Services.AddControllers();
            builder.Services.AddGrpc();

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Add OpenAPI/Swagger
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<Usuarios.Persistence.Data.BarceloIoTSystemContext>();
                await context.Database.EnsureCreatedAsync();
            }

            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseRouting();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            // REST endpoints
            app.MapControllers();

            // gRPC endpoints
            app.MapGrpcService<HuespedeGrpcService>();

            await app.RunAsync();
        }
    }
}
