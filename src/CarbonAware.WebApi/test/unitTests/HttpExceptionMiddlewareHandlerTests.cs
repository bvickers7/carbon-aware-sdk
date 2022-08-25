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

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public async Task InvokeAsyncSuccessTest()
    {
        var mockHttpContext = CreateMockHttpContext();
        var mockRequestDelegate = new Mock<RequestDelegate>();
        mockRequestDelegate.Setup(requestDelegate => requestDelegate.Invoke(mockHttpContext.Object));
        var mockLogger = new Mock<ILogger<HttpExceptionMiddlewareHandler>>();
        var mockOptions = new Mock<IOptionsMonitor<CarbonAwareVariablesConfiguration>>();
        var middleware = new HttpExceptionMiddlewareHandler(mockRequestDelegate.Object, mockLogger.Object, mockOptions.Object);
        await middleware.InvokeAsync(mockHttpContext.Object);

        mockRequestDelegate.Verify(requestDelegate => requestDelegate.Invoke(mockHttpContext.Object), Times.Once);
    }

    private Mock<HttpContext> CreateMockHttpContext()
    {
        var mockHttpResponse = new Mock<HttpResponse>();
        mockHttpResponse.SetupGet(resp => resp.Body).Returns(new MemoryStream());
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.SetupGet(ctx => ctx.Response).Returns(mockHttpResponse.Object);
        return mockHttpContext;
    }
}
