using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using CarbonAware.Interfaces;
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
        var response = CreateProblemDetailsResponse(context, exception);
        context.Response.Clear();
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)response.Status!;
        var jsonResponse = JsonSerializer.Serialize<HttpValidationProblemDetails>(response);
        await context.Response.WriteAsync(jsonResponse);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        await buffer.CopyToAsync(stream);
        context.Response.Body = stream;
    }

    private HttpValidationProblemDetails CreateProblemDetailsResponse(HttpContext context, Exception exception)
    {
        var activity = Activity.Current;
        HttpValidationProblemDetails response;
        if (exception is IHttpResponseException httpResponseException)
        {
            response = new HttpValidationProblemDetails(){
                Title = httpResponseException.Title,
                Status = httpResponseException.Status,
                Detail = httpResponseException.Detail
            };
        } else {
            var exceptionType = exception.GetType().Name;
            int statusCode;
            if (!EXCEPTION_STATUS_CODE_MAP.TryGetValue(exceptionType, out statusCode))
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            }
            var isVerboseApi = _options.CurrentValue.VerboseApi;
       
            if (statusCode == (int)HttpStatusCode.InternalServerError && !isVerboseApi)
            {
                 response = new HttpValidationProblemDetails() {
                                Title = HttpStatusCode.InternalServerError.ToString(),
                                Status = statusCode,
                    };
            }
            else
            {
                response = new HttpValidationProblemDetails() {
                            Title = exceptionType,
                            Status = statusCode,
                            Detail = exception.Message
                };
                if (isVerboseApi) {
                    response.Errors["stackTrace"] = new string[] { exception.StackTrace! };
                }
            }
        }

        var traceId = activity?.Id;
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
        return response;
    }
}
