using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace CarbonAware.WebApi.Middleware;

public class HttpExceptionMiddlewareHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpExceptionMiddlewareHandler> _logger;
    private IOptionsMonitor<CarbonAwareVariablesConfiguration> _options;

    public HttpExceptionMiddlewareHandler(RequestDelegate next, ILogger<HttpExceptionMiddlewareHandler> logger, IOptionsMonitor<CarbonAwareVariablesConfiguration> options)
    {
        _logger = logger;
        _next = next;
        _options = options;
    }
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong: {ex}");
            await HandleExceptionAsync(httpContext, ex);
        }
    }
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
         var response = new HttpValidationProblemDetails() {
                            Title = exception.GetType().ToString(),
                            Status = (int)HttpStatusCode.InternalServerError,
                            Detail = exception.Message
                };
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await context.Response.WriteAsync(JsonSerializer.Serialize<HttpValidationProblemDetails>(response));
    }
}