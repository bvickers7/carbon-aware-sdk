using System.Net;
using System.Text.Json;
using CarbonAware.WebApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;


namespace CarbonAware.WepApi.UnitTests;

/// <summary>
/// Tests Http Exceptions
/// </summary>
[TestFixture]
public class HttpExceptionMiddlewareHandlerTests
{

    [Test]
    public async Task InvokeAsync_SuccessTest()
    {
        var context = CreateDefaultHttpContext();
        var mockRequestDelegate = new Mock<RequestDelegate>();
        mockRequestDelegate.Setup(x => x.Invoke(context));
        var mockLogger = new Mock<ILogger<HttpExceptionMiddlewareHandler>>();
        var mockOptions = new Mock<IOptionsMonitor<CarbonAwareVariablesConfiguration>>();
        var middleware = new HttpExceptionMiddlewareHandler(mockRequestDelegate.Object, mockLogger.Object, mockOptions.Object);
        await middleware.InvokeAsync(context);

        mockRequestDelegate.Verify(requestDelegate => requestDelegate.Invoke(context), Times.Once);
    }

    [Test]
    public async Task InvokeAsync_WithException()
    {
        var context = CreateDefaultHttpContext();
        var mockRequestDelegate = SetupRequestDelegate(context, new Exception("New Exception"));
        var mockLogger = new Mock<ILogger<HttpExceptionMiddlewareHandler>>();
        var mockOptions = new Mock<IOptionsMonitor<CarbonAwareVariablesConfiguration>>();
        mockOptions.Setup(x => x.CurrentValue).Returns(new CarbonAwareVariablesConfiguration() { VerboseApi = false });
        var middleware = new HttpExceptionMiddlewareHandler(mockRequestDelegate.Object, mockLogger.Object, mockOptions.Object);
        await middleware.InvokeAsync(context);

        Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
        Assert.That(context.Response.ContentType, Is.EqualTo("application/problem+json"));
        var result = await GetProblemDetailsFromContext(context);
        Assert.NotNull(result);
        Assert.That(result!.Title, Is.EqualTo(HttpStatusCode.InternalServerError.ToString()));
        Assert.That(result!.Status, Is.EqualTo((int)HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task InvokeAsync_WithArgumentException()
    {
        var context = CreateDefaultHttpContext();
        var msg = "Testing ArgumentException";
        var expectedException = new ArgumentException(msg);
        var mockRequestDelegate = SetupRequestDelegate(context, expectedException);
        var mockLogger = new Mock<ILogger<HttpExceptionMiddlewareHandler>>();
        var mockOptions = new Mock<IOptionsMonitor<CarbonAwareVariablesConfiguration>>();
        mockOptions.Setup(x => x.CurrentValue).Returns(new CarbonAwareVariablesConfiguration() { VerboseApi = false });
        var middleware = new HttpExceptionMiddlewareHandler(mockRequestDelegate.Object, mockLogger.Object, mockOptions.Object);
        await middleware.InvokeAsync(context);

        Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
        Assert.That(context.Response.ContentType, Is.EqualTo("application/problem+json"));
        var result = await GetProblemDetailsFromContext(context);
        Assert.NotNull(result);
        Assert.That(result!.Title, Is.EqualTo(expectedException.GetType().Name));
        Assert.That(result!.Status, Is.EqualTo((int)HttpStatusCode.BadRequest));
        Assert.That(result!.Detail, Is.EqualTo(msg));
    }

    private Mock<HttpContext> CreateMockHttpContext()
    {
        var mockHttpResponse = new Mock<HttpResponse>();
        mockHttpResponse.SetupGet(x => x.Body).Returns(new MemoryStream());
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.SetupGet(x => x.Response).Returns(mockHttpResponse.Object);
        return mockHttpContext;
    }

    private static HttpContext CreateDefaultHttpContext()
    {
        var result = new DefaultHttpContext();
        result.Response.Body = new MemoryStream();
        return result;
    }

    private static async Task<HttpValidationProblemDetails?> GetProblemDetailsFromContext(HttpContext context)
    {
        var stream = context.Response.Body;
        stream.Seek(0, SeekOrigin.Begin);
        var data = await JsonSerializer.DeserializeAsync<HttpValidationProblemDetails>(stream);
        return data;
    }

    private static Mock<RequestDelegate> SetupRequestDelegate(HttpContext context, Exception expectedException)
    {
        var mockRequestDelegate = new Mock<RequestDelegate>();
        mockRequestDelegate.Setup(x => x.Invoke(context)).ThrowsAsync(expectedException);
        return mockRequestDelegate;
    }
}
