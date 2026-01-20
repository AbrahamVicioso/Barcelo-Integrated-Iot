using Authentication.Domain.Entities;
using Authentication.Persistence;
using Authentication.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddPersistenceLayer();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configurar Identity ANTES de Authentication
builder.Services
    .AddIdentityCore<User>(options =>
    {
        options.Lockout.AllowedForNewUsers = false;

        // Configuración de Sign In
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedAccount = false;

        // Configuración de Password (ajusta según tus necesidades)
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<AuthenticationDbContext>()
    .AddApiEndpoints();

// Authentication debe ir DESPUÉS de Identity
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
        options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
        options.DefaultScheme = IdentityConstants.BearerScheme;
    })
    .AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// ORDEN CORRECTO DEL MIDDLEWARE
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// MapIdentityApi y MapControllers deben ir al final
app.MapIdentityApi<User>();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AuthenticationDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();