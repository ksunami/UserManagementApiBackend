using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.Options;
using UserApi.Middleware;
using UserApi.Options;


namespace UserApi.Tests;

public class RequestResponseLoggingMiddlewareTests
{
    [Fact]
    public async Task Middleware_LogsRequestAndResponse()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var middleware = new RequestResponseLoggingMiddleware(async (innerHttpContext) =>
        {
            innerHttpContext.Response.StatusCode = 200;
            await innerHttpContext.Response.WriteAsync("Test response");
        }, loggerMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/test";
        context.Request.QueryString = new QueryString("?id=123");
        context.Request.Headers["Test-Header"] = "HeaderValue";

        var requestBody = "{\"name\":\"Ken\",\"email\":\"ken@example.com\",\"password\":\"secret\"}";
        var requestBytes = Encoding.UTF8.GetBytes(requestBody);
        context.Request.Body = new MemoryStream(requestBytes);
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.Invoke(context);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}