using FluentValidation;
using Ledger.Application.Common;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Ledger.Api.Middleware;

public partial class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Validation failed: {Errors}")]
    private static partial void LogValidationFailed(ILogger logger, string errors);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Resource not found: {Message}")]
    private static partial void LogNotFound(ILogger logger, string message);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception")]
    private static partial void LogUnhandledException(ILogger logger, Exception ex);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            LogValidationFailed(logger, ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            var errors = ex.Errors.Select(e => e.ErrorMessage).ToArray();
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(ApiResponse.Fail(errors), JsonOptions));
        }
        catch (KeyNotFoundException ex)
        {
            LogNotFound(logger, ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(ApiResponse.Fail(ex.Message), JsonOptions));
        }
        catch (Exception ex)
        {
            LogUnhandledException(logger, ex);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(
                    ApiResponse.Fail("Something went wrong in the multiverse."),
                    JsonOptions));
        }
    }
}
