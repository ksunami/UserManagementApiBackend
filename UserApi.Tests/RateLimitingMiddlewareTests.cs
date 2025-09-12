using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using UserApi.Middleware;
using Xunit;
using Microsoft.Extensions.Options;
using UserApi.Options;

namespace UserApi.Tests;

public class RateLimitingMiddlewareTests
{
    private readonly string _clientIp = "127.0.0.1";

    private DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(_clientIp);
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task Allows_Request_Within_Limit()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RateLimitingMiddleware>>();
        var options = Microsoft.Extensions.Options.Options.Create(new RateLimitingOptions { Limit = 5, WindowMinutes = 1 });

        var middleware = new RateLimitingMiddleware(
            async ctx => await ctx.Response.WriteAsync("OK"),
            loggerMock.Object,
            options);

        var context = CreateContext();

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(context.Response.Body).ReadToEndAsync();

        Assert.Equal("OK", response);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task Blocks_Request_Exceeding_Limit()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RateLimitingMiddleware>>();
        var options = Microsoft.Extensions.Options.Options.Create(new RateLimitingOptions { Limit = 3, WindowMinutes = 1 });

        var middleware = new RateLimitingMiddleware(
            async ctx => await ctx.Response.WriteAsync("OK"),
            loggerMock.Object,
            options);

        // Simulate 4 requests from the same IP
        for (int i = 0; i < 3; i++)
        {
            var context = CreateContext();
            await middleware.Invoke(context);
        }

        var blockedContext = CreateContext();
        await middleware.Invoke(blockedContext);

        // Assert
        blockedContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(blockedContext.Response.Body).ReadToEndAsync();

        Assert.Contains("Rate limit exceeded", response);
        Assert.Equal(StatusCodes.Status429TooManyRequests, blockedContext.Response.StatusCode);
    }
}