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

public class AuthenticationMiddlewareTests
{
    private const string ValidToken = "my-secret-token-123";

    private DefaultHttpContext CreateContext(string? token = null)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        if (token != null)
        {
            context.Request.Headers["Authorization"] = $"Bearer {token}";
        }

        return context;
    }

    [Fact]
    public async Task Allows_Request_With_Valid_Token()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AuthenticationMiddleware>>();
        var options = Microsoft.Extensions.Options.Options.Create(new AuthenticationOptions { Token = ValidToken });

        var middleware = new AuthenticationMiddleware(
            async ctx => await ctx.Response.WriteAsync("Authorized"),
            loggerMock.Object,
            options);

        var context = CreateContext(ValidToken);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(context.Response.Body).ReadToEndAsync();

        Assert.Equal("Authorized", response);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task Blocks_Request_With_Missing_Token()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AuthenticationMiddleware>>();
        var options = Microsoft.Extensions.Options.Options.Create(new AuthenticationOptions { Token = ValidToken });

        var middleware = new AuthenticationMiddleware(
            async ctx => await ctx.Response.WriteAsync("Authorized"),
            loggerMock.Object,
            options);

        var context = CreateContext(); // no token

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(context.Response.Body).ReadToEndAsync();

        Assert.Contains("Unauthorized", response);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task Blocks_Request_With_Invalid_Token()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AuthenticationMiddleware>>();
        var options = Microsoft.Extensions.Options.Options.Create(new AuthenticationOptions { Token = ValidToken });

        var middleware = new AuthenticationMiddleware(
            async ctx => await ctx.Response.WriteAsync("Authorized"),
            loggerMock.Object,
            options);

        var context = CreateContext("wrong-token");

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(context.Response.Body).ReadToEndAsync();

        Assert.Contains("Unauthorized", response);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }
}