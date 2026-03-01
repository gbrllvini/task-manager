using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace TaskManager.API.Middlewares;

public class ExceptionMiddleware {
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger) {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context) {
        try {
            await _next(context);
        }
        catch (Exception exception) {
            _logger.LogError(exception, "Unhandled exception while processing request.");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception) {
        var (statusCode, title) = exception switch {
            ArgumentException => (HttpStatusCode.BadRequest, "Bad Request"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Not Found"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails {
            Status = context.Response.StatusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}
