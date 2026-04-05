using Audit.Worker;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Audit Consumer Config
var auditConsumerConfig = new AuditConsumerConfig();
builder.Configuration.GetSection("AuditConsumer").Bind(auditConsumerConfig);
builder.Services.AddSingleton(auditConsumerConfig);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Repositories
builder.Services.AddSingleton<IAuditRepository>(new AuditRepository(connectionString));
builder.Services.AddSingleton<IAuditQueryRepository>(new AuditQueryRepository(connectionString));

// Kafka consumer background service
builder.Services.AddHostedService<AuditEventConsumer>();

// API
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("Audit API - Barceló IoT");
    options.WithTheme(ScalarTheme.Purple);
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseRouting();
app.MapControllers();
app.Run();
