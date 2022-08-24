using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace CarbonAware.WebApi.Middleware;

public class HttpExceptionMiddlewareHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpExceptionMiddlewareHandler> _logger;
    private IOptionsMonitor<CarbonAwareVariablesConfiguration> _options;

    private static Dictionary<string, int> EXCEPTION_STATUS_CODE_MAP = new Dictionary<string, int>()
    {
        { "ArgumentException", (int)HttpStatusCode.BadRequest },
        { "NotImplementedException", (int)HttpStatusCode.NotImplemented },
    };

    public HttpExceptionMiddlewareHandler(RequestDelegate next, ILogger<HttpExceptionMiddlewareHandler> logger, IOptionsMonitor<CarbonAwareVariablesConfiguration> options)
    {
        _logger = logger;
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        using (var buffer = new MemoryStream()) {
            var stream = httpContext.Response.Body;
            try
            {
                httpContext.Response.Body = buffer;
                await _next(httpContext);
                httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
                await buffer.CopyToAsync(stream);
                httpContext.Response.Body = stream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: {exception}", ex.Message);
                await HandleExceptionAsync(httpContext, ex, buffer, stream);
            }
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, MemoryStream buffer, Stream stream)
    {
        var response = new HttpValidationProblemDetails()
        {
            Title = exception.GetType().ToString(),
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = exception.Message
        };

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        if (traceId != null)
        {
            response.Extensions["traceId"] = traceId;
        }

        foreach (DictionaryEntry entry in exception.Data)
        {
            if (entry.Value is string[] messages && entry.Key is string key){
                response.Errors[key] = messages;
            }
        }
        context.Response.Clear();
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var json = JsonSerializer.Serialize<HttpValidationProblemDetails>(response);
        await context.Response.WriteAsync(json);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        await buffer.CopyToAsync(stream);
        context.Response.Body = stream;
    }
}
