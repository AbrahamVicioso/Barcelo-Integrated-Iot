using Authentication.Api;
using Authentication.Api.Services;
using Grpc.AspNetCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

app.Run();
