using Order_MS.DTOs;
using Order_MS.Exceptions;
using System.Net;
using System.Text.Json;

namespace Order_MS.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Let the request proceed normally
            await _next(context);
        }
        catch (BusinessException ex)
        {
            // Expected business error (404 not found, 400 bad request)
            _logger.LogWarning(ex, "Business rule violation: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            // Unexpected crash (database down, null reference, etc.)
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            // In Development, show details. In Production, hide them.
            var message = _env.IsDevelopment()
                ? ex.Message
                : "An unexpected error occurred. Please try again later.";

            await HandleExceptionAsync(context, 500, message, ex.StackTrace);
        }
    }

    private static Task HandleExceptionAsync(
        HttpContext context,
        int statusCode,
        string message,
        string? details = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            Details = details  // Only populated in Development
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(response, jsonOptions);
        return context.Response.WriteAsync(json);
    }
}