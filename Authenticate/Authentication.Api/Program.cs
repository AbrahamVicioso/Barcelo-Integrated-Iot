using Authentication.Api;
using Authentication.Api.Data;
using Authentication.Api.Services;
using Authentication.Domain.Entities;
using Grpc.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Puerto 5117: HTTP/1.1 para REST
// Puerto 5118: HTTP/2 exclusivo para gRPC
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5117, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
    options.ListenLocalhost(5118, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
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

// AUTO GENERATE DB (FOR DEMO PURPOSES ONLY)
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var context = services.GetRequiredService<AuthenticationDbContext>();
//    await context.Database.EnsureCreatedAsync();
//}

using (var scope = app.Services.CreateScope())
{
    var userManager  = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager  = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await DbSeeder.SeedAsync(userManager, roleManager);
}

app.Run();
