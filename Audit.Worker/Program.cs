using Audit.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Audit Consumer Config
var auditConsumerConfig = new AuditConsumerConfig();
builder.Configuration.GetSection("AuditConsumer").Bind(auditConsumerConfig);
builder.Services.AddSingleton(auditConsumerConfig);

// Audit Repository (raw SqlClient)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddSingleton<IAuditRepository>(new AuditRepository(connectionString));

// Hosted worker
builder.Services.AddHostedService<AuditEventConsumer>();

var host = builder.Build();
host.Run();
