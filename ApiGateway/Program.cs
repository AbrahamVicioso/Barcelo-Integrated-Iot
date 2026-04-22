using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Scalar.AspNetCore;

namespace ApiGateway;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // HTTP client used to proxy OpenAPI specs from downstream services
        builder.Services.AddHttpClient("openapi", client =>
            client.Timeout = TimeSpan.FromSeconds(10));

        builder.Configuration
            .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

        builder.Services.AddOcelot();

        var allowedOrigins = builder.Configuration
            .GetSection("AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"];

        builder.Services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials()));

        var app = builder.Build();

        // ── Documentation service registry ────────────────────────────────────
        var docServices = app.Configuration
            .GetSection("ApiDocumentation:Services")
            .Get<List<ApiDocService>>() ?? [];

        // ── OpenAPI spec proxy ────────────────────────────────────────────────
        // Serves each downstream /openapi/v1.json through the gateway so Scalar
        // can reach them from the same origin (no CORS issues).
        // To add a new service: add an entry to ApiDocumentation:Services in appsettings.json.
        app.MapGet("/openapi/{service}/v1.json",
            async (string service, IHttpClientFactory factory) =>
            {
                var svc = docServices.Find(s =>
                    s.Name.Equals(service, StringComparison.OrdinalIgnoreCase));

                if (svc is null)
                    return Results.NotFound();

                var client = factory.CreateClient("openapi");
                try
                {
                    var response = await client.GetAsync($"{svc.BaseUrl}/openapi/v1.json");
                    var json = await response.Content.ReadAsStringAsync();
                    return Results.Content(json, "application/json");
                }
                catch
                {
                    return Results.Problem(
                        title: $"Service '{service}' is unavailable.",
                        statusCode: 503);
                }
            });

        // ── Scalar UI (dev + Docker only) ─────────────────────────────────────
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
        {
            // One Scalar page per service → /scalar/{serviceName}
            foreach (var svc in docServices)
            {
                var capturedSvc = svc;
                app.MapScalarApiReference($"/scalar/{svc.Name}", options =>
                    options
                        .WithTitle($"Barcelo IoT – {capturedSvc.DisplayName}")
                        .WithTheme(ScalarTheme.Saturn)
                        .WithOpenApiRoutePattern($"/openapi/{capturedSvc.Name}/v1.json"));
            }

            // Central index listing all services → /docs
            app.MapGet("/docs", async ctx =>
            {
                var cards = string.Join("\n", docServices.Select(s =>
                    $"""        <a href="/scalar/{s.Name}" class="card">{s.DisplayName}</a>"""));

                ctx.Response.ContentType = "text/html; charset=utf-8";
                await ctx.Response.WriteAsync($$"""
                    <!DOCTYPE html>
                    <html lang="en">
                    <head>
                      <meta charset="UTF-8">
                      <meta name="viewport" content="width=device-width, initial-scale=1.0">
                      <title>Barcelo IoT — API Reference</title>
                      <style>
                        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
                        body { font-family: system-ui, -apple-system, sans-serif; background: #0c0c0e; color: #e2e2e6; min-height: 100dvh; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 2.5rem; padding: 2rem; }
                        h1 { font-size: 1.75rem; font-weight: 700; letter-spacing: -.4px; }
                        p  { color: #777; font-size: .875rem; margin-top: .4rem; }
                        .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: .875rem; width: 100%; max-width: 800px; }
                        .card { display: flex; align-items: center; justify-content: center; height: 80px; border-radius: 10px; border: 1px solid #222; background: #141416; color: #e2e2e6; font-weight: 500; text-decoration: none; transition: border-color .15s, background .15s; }
                        .card:hover { border-color: #6c6ef7; background: #18182a; color: #fff; }
                      </style>
                    </head>
                    <body>
                      <div style="text-align:center">
                        <h1>Barcelo Integrated IoT</h1>
                        <p>Select a service to browse its API reference</p>
                      </div>
                      <div class="grid">
                    {{cards}}
                      </div>
                    </body>
                    </html>
                    """);
            });
        }

        // ── Middleware pipeline ───────────────────────────────────────────────
        //
        // UseOcelot() is terminal middleware that intercepts every request.
        // To serve Scalar and the OpenAPI proxy before it we:
        //   1. Call UseRouting() explicitly so endpoint matching happens here in
        //      the pipeline, before Ocelot runs.
        //   2. Add an interceptor that executes any matched local endpoint
        //      (Scalar, OpenAPI proxy, /docs) and short-circuits the pipeline.
        //   3. Requests with no matched endpoint fall through to Ocelot.
        //
        // Explicit preflight handler — must run before Ocelot and UseRouting.
        // Ocelot has a known issue where it intercepts OPTIONS before ASP.NET Core
        // CORS middleware can short-circuit it.
        app.Use(async (ctx, next) =>
        {
            if (HttpMethods.IsOptions(ctx.Request.Method) &&
                ctx.Request.Headers.ContainsKey("Origin"))
            {
                var origin = ctx.Request.Headers.Origin.ToString();
                if (allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                {
                    ctx.Response.Headers.Append("Access-Control-Allow-Origin", origin);
                    ctx.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS, PATCH");
                    ctx.Response.Headers.Append("Access-Control-Allow-Headers",
                        ctx.Request.Headers.TryGetValue("Access-Control-Request-Headers", out var reqHeaders)
                            ? reqHeaders.ToString()
                            : "Content-Type, Authorization");
                    ctx.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
                    ctx.Response.Headers.Append("Access-Control-Max-Age", "86400");
                    ctx.Response.StatusCode = StatusCodes.Status204NoContent;
                    return;
                }
            }
            await next();
        });

        app.UseCors();

        app.UseRouting();

        app.Use(async (ctx, next) =>
        {
            if (ctx.GetEndpoint() is { } endpoint)
            {
                await endpoint.RequestDelegate!(ctx);
                return;
            }
            await next();
        });

        app.UseWebSockets();
        app.UseAuthorization();

        // Propagate the gateway's public address to downstream services so they
        // can build correct public-facing URLs (e.g. email confirmation links).
        app.Use(async (ctx, next) =>
        {
            ctx.Request.Headers["X-Forwarded-Proto"] = ctx.Request.Scheme;
            ctx.Request.Headers["X-Forwarded-Host"] = ctx.Request.Host.Value;
            await next(ctx);
        });

        await app.UseOcelot();

        app.Run();
    }
}

/// <summary>Downstream service registered for centralised API documentation.</summary>
public record ApiDocService(string Name, string DisplayName, string BaseUrl);
