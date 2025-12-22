// Middleware/ExceptionHandlingMiddleware.cs

using System.Net;
using System.Text.Json;
using PracticalWork.Library.Exceptions;

namespace PracticalWork.Library.Web.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        _logger.LogError(exception, "Произошла ошибка при обработке запроса");

        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            BookServiceException => (HttpStatusCode.BadRequest, exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Ресурс не найден"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Доступ запрещен"),
            _ => (HttpStatusCode.InternalServerError, "Внутренняя ошибка сервера")
        };

        response.StatusCode = (int)statusCode;

        var errorResponse = new
        {
            StatusCode = response.StatusCode,
            Message = message,
            Details = exception.Message,
            Timestamp = DateTime.UtcNow
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse);
        await response.WriteAsync(jsonResponse);
    }
}