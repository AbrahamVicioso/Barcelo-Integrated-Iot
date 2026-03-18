using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notification.Domain.Interfaces;
using Scalar.AspNetCore;
using Usuarios.Application.Behaviors;
using Usuarios.Application.Mappings;
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
            var builder = WebApplication.CreateBuilder(args);

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

            // Add Controllers
            builder.Services.AddControllers();

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

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
