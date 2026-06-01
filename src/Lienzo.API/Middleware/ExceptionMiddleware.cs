using System.Net;
using FluentValidation;
using Serilog;

namespace Lienzo.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleUnauthorizedAccessExceptionAsync(context, ex);
        }
        catch (KeyNotFoundException ex)
        {
            await HandleKeyNotFoundExceptionAsync(context, ex);
        }
        catch (ArgumentException ex)
        {
            await HandleArgumentExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleInternalServerErrorAsync(context, ex);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        Log.Warning(ex, "Validation failed");
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Title = "Validation Failed",
            Detail = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
            Status = (int)HttpStatusCode.UnprocessableEntity,
            Type = "https://tools.ietf.org/html/rfc4918",
            Instance = context.Request.Path
        };

        problemDetails.Extensions["errors"] = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static async Task HandleUnauthorizedAccessExceptionAsync(HttpContext context, UnauthorizedAccessException ex)
    {
        Log.Warning(ex, "Unauthorized access attempt");
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Title = "Forbidden",
            Detail = ex.Message,
            Status = (int)HttpStatusCode.Forbidden,
            Type = "https://tools.ietf.org/html/rfc7231",
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static async Task HandleKeyNotFoundExceptionAsync(HttpContext context, KeyNotFoundException ex)
    {
        Log.Warning(ex, "Resource not found");
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Title = "Not Found",
            Detail = ex.Message,
            Status = (int)HttpStatusCode.NotFound,
            Type = "https://tools.ietf.org/html/rfc7231",
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static async Task HandleArgumentExceptionAsync(HttpContext context, ArgumentException ex)
    {
        Log.Warning(ex, "Bad request");
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Title = "Bad Request",
            Detail = ex.Message,
            Status = (int)HttpStatusCode.BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231",
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static async Task HandleInternalServerErrorAsync(HttpContext context, Exception ex)
    {
        Log.Error(ex, "An unhandled exception occurred");
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred. Please try again later.",
            Status = (int)HttpStatusCode.InternalServerError,
            Type = "https://tools.ietf.org/html/rfc7231",
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
