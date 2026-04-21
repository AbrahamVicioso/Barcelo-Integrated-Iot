using Authentication.Api;
using Authentication.Api.Data;
using Authentication.Api.Services;
using Authentication.Domain.Entities;
using Grpc.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Notification.Kafka.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Puerto 5117: HTTP/1.1 para REST
// Puerto 5118: HTTP/2 para gRPC (HTTPS cuando hay cert configurado, h2c en dev local)
builder.WebHost.ConfigureKestrel(options =>
{
    var certPath = builder.Configuration["Kestrel:Certificates:Default:Path"];
    var certPassword = builder.Configuration["Kestrel:Certificates:Default:Password"];

    options.ListenAnyIP(5117, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    options.ListenAnyIP(5118, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
        if (!string.IsNullOrEmpty(certPath))
            listenOptions.UseHttps(certPath, certPassword);
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddAuthorizationConfiguration();
builder.Services.AddServicesDependency(builder.Configuration);

// Add gRPC services
builder.Services.AddGrpc();

//builder.Host.UseWolverine();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add NotificacionDbContext for preferencias and historial
var notificationConnectionString = builder.Configuration.GetConnectionString("BarceloIoTDatabase")
    ?? throw new InvalidOperationException("Missing required configuration 'ConnectionStrings:BarceloIoTDatabase'");
builder.Services.AddDbContext<NotificacionDbContext>(options =>
    options.UseSqlServer(notificationConnectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    app.MapGet("/", () => Results.Redirect("/scalar"));
}

//app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map gRPC service
app.MapGrpcService<UserLookupService>();

using (var scope = app.Services.CreateScope())
{
    var context      = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
    await context.Database.EnsureCreatedAsync();

    var userManager  = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager  = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await DbSeeder.SeedAsync(userManager, roleManager, configuration);
}

app.Run();
