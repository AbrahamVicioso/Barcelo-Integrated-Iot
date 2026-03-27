using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Reservas.API;
using Reservas.Application;
using Reservas.Email;
using Reservas.Infrastructure;
using Reservas.Persistence;
using Reservas.Persistence.Data;
using Scalar.AspNetCore;
using System.Security.Claims;

// Habilitar HTTP/2 sin cifrado (h2c) para clientes gRPC internos
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApi();

builder.Services.AddAuthorizationServices();

// Register Application and Persistence layers
builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BarceloReservasContext>();
    await context.Database.EnsureCreatedAsync();
    await Reservas.Persistence.DbSeeder.SeedAsync(context);
}

app.MapControllers();

app.Run();
