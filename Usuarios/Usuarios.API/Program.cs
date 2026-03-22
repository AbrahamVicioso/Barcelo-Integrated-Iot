using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notification.Domain.Interfaces;
using Scalar.AspNetCore;
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
        public static void Main(string[] args)
        {
            // Habilitar HTTP/2 sin cifrado (h2c) para clientes gRPC internos
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var builder = WebApplication.CreateBuilder(args);

            // Forzar soporte de HTTP/2 cleartext (h2c) en Kestrel para gRPC sin TLS
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureEndpointDefaults(listenOptions =>
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                });
            });

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

            // Required for AuditBehavior
            builder.Services.AddHttpContextAccessor();

            // Add FluentValidation
            builder.Services.AddValidatorsFromAssembly(typeof(Usuarios.Application.Validators.Huespedes.CreateHuespedeDtoValidator).Assembly);

            // Add Repositories and Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IHuespedeRepository, HuespedeRepository>();
            builder.Services.AddScoped<IPersonalRepository, PersonalRepository>();
            builder.Services.AddScoped<IPermisosPersonalRepository, PermisosPersonalRepository>();

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

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowAll");

            app.UseAuthorization();

            // REST endpoints
            app.MapControllers();

            // gRPC endpoints
            app.MapGrpcService<HuespedeGrpcService>();

            app.Run();
        }
    }
}
