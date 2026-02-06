
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Reservas.API;
using Reservas.Application;
using Reservas.Application.Interfaces;
using Reservas.Application.Services;
using Reservas.Email;
using Reservas.Infrastructure;
using Reservas.Persistence;
using Reservas.Persistence.Data;
using Scalar.AspNetCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthorizationServices();

// Register Application and Persistence layers
builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceLayer();
builder.Services.AddEmailServices(builder.Configuration);
builder.Services.AddInfrastructureLayer(builder.Configuration);

// Configure HttpClient for Usuarios API
builder.Services.AddHttpClient<IUsuariosApiService, UsuariosApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Apis:Usuarios:BaseUrl"] ?? "http://localhost:5284/");
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BarceloReservasContext>();
    await context.Database.EnsureCreatedAsync();
}

app.MapControllers();

app.Run();