using System.Diagnostics;
using Serilog;

namespace Lienzo.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        Log.Information("Incoming request {Method} {Path}", requestMethod, requestPath);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;

            Log.Information(
                "Outgoing response {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                requestMethod, requestPath, statusCode, elapsed);
        }
    }
}
