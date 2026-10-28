using System.Net;
using System.Text.Json;
using IncidentFlow.Application.Common.Exceptions;
using IncidentFlow.Application.Common.Models;

namespace IncidentFlow.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
        var (statusCode, response) = exception switch
        {
            FluentValidation.ValidationException fluentEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(
                    "One or more validation failures have occurred.",
                    fluentEx.Errors.Select(e => e.ErrorMessage).Distinct())),
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(
                    validationEx.Message,
                    validationEx.Errors.SelectMany(e => e.Value))),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.Fail(notFoundEx.Message)),
            ForbiddenAccessException forbiddenEx => (
                HttpStatusCode.Forbidden,
                ApiResponse<object>.Fail(forbiddenEx.Message)),
            BusinessRuleException businessEx => (
                HttpStatusCode.Conflict,
                ApiResponse<object>.Fail(businessEx.Message)),
            UnauthorizedAccessException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.Fail(unauthorizedEx.Message)),
            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail("An unexpected error occurred."))
        };

        if ((int)statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception for {Method} {Path}: {Message}",
                context.Request.Method, context.Request.Path, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(payload);
    }
}
