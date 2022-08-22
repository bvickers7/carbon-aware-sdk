using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CarbonAware.WebApi.Middleware;

public static class HttpExceptionMiddlewareHandler
{
    public static void UseCustomErrors(this IApplicationBuilder app)
    {
        app.Use(HandleResponse);
    }

    private static Task HandleResponse(HttpContext httpContext, Func<Task> next)
        => WriteResponse(httpContext);

    private static async Task WriteResponse(HttpContext httpContext)
    {
        var exceptionDetails = httpContext.Features.Get<IExceptionHandlerFeature>();
        var ex = exceptionDetails?.Error;

        if (ex != null)
        {
            httpContext.Response.ContentType = "application/problem+json";

            var title = "An error occured: " + ex.Message;
            var details = ex.ToString();
            
            var problem = new ProblemDetails
            {
                Status = 500,
                Title = title,
                Detail = details
            };

            var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
            if (traceId != null)
            {
                problem.Extensions["traceId"] = traceId;
            }

            var stream = httpContext!.Response.Body;
            await JsonSerializer.SerializeAsync(stream, problem);
        }
    }
}