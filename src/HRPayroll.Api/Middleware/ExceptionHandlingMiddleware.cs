using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using HRPayroll.Domain.Exceptions;

namespace HRPayroll.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly bool _isDevelopment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _isDevelopment = environment.IsDevelopment();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = MapException(exception);

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Type} — {Message}",
                exception.GetType().Name, exception.Message);
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            detail,
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(problemDetails, JsonSerializerOptions.Web);
        await context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode StatusCode, string Title, string Detail) MapException(Exception exception)
    {
        return exception switch
        {
            DomainException domain => (
                HttpStatusCode.Conflict,
                "DomainRuleViolation",
                domain.Message),

            AuthenticationException => (
                HttpStatusCode.Unauthorized,
                "Unauthorized",
                exception.Message),

            UnauthorizedAccessException => (
                HttpStatusCode.Forbidden,
                "Forbidden",
                exception.Message),

            _ => (
                HttpStatusCode.InternalServerError,
                "InternalServerError",
                exception.ToString())
        };
    }
}
