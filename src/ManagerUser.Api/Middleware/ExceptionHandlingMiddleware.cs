using FluentValidation;
using ManagerUser.Application.Common.Exceptions;
using ManagerUser.Contracts.Common;
using System.Net;
using System.Text.Json;

namespace ManagerUser.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
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
        catch (ValidationException ex)
        {
            await WriteValidationErrorAsync(context, ex);
        }
        catch (AppException ex)
        {
            await WriteAppErrorAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled API error.");
            await WriteUnhandledErrorAsync(context);
        }
    }

    private static Task WriteValidationErrorAsync(HttpContext context, ValidationException exception)
    {
        var errors = exception.Errors
            .Select(error => new ApiError(error.PropertyName, error.ErrorMessage))
            .ToList();

        return WriteAsync(
            context,
            HttpStatusCode.BadRequest,
            ApiResponse<object>.Fail(
                "VALIDATION_ERROR",
                "Dữ liệu không hợp lệ.",
                errors,
                context.TraceIdentifier));
    }

    private static Task WriteAppErrorAsync(HttpContext context, AppException exception)
    {
        var errors = exception.Errors
            .SelectMany(item => item.Value.Select(message => new ApiError(item.Key, message)))
            .ToList();

        return WriteAsync(
            context,
            exception.StatusCode,
            ApiResponse<object>.Fail(
                exception.Code,
                exception.Message,
                errors,
                context.TraceIdentifier));
    }

    private static Task WriteUnhandledErrorAsync(HttpContext context)
    {
        return WriteAsync(
            context,
            HttpStatusCode.InternalServerError,
            ApiResponse<object>.Fail(
                "INTERNAL_SERVER_ERROR",
                "Hệ thống đang gặp lỗi.",
                Array.Empty<ApiError>(),
                context.TraceIdentifier));
    }

    private static async Task WriteAsync<T>(
        HttpContext context,
        HttpStatusCode statusCode,
        ApiResponse<T> response)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        await JsonSerializer.SerializeAsync(context.Response.Body, response, JsonOptions);
    }
}
