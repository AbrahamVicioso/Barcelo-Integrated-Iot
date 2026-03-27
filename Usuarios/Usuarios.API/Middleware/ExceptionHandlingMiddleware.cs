using System.Text.Json;
using Usuarios.Application.Exceptions;

namespace Usuarios.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteErrorResponse(context, ex);
        }
    }

    private static Task WriteErrorResponse(HttpContext context, Exception ex)
    {
        var (statusCode, error) = ex switch
        {
            NotFoundException    => (StatusCodes.Status404NotFound,       "Not Found"),
            ConflictException    => (StatusCodes.Status409Conflict,       "Conflict"),
            BusinessException    => (StatusCodes.Status400BadRequest,     "Bad Request"),
            _                    => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = statusCode;

        var body = JsonSerializer.Serialize(new
        {
            status  = statusCode,
            error,
            message = ex.Message
        });

        return context.Response.WriteAsync(body);
    }
}
