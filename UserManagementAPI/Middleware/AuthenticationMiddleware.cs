using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserApi.Options;

namespace UserApi.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private const string AUTH_HEADER = "Authorization";
    private const string BEARER_PREFIX = "Bearer ";
    private readonly string _validToken;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger,IOptions<AuthenticationOptions> options)
    {
        _next = next;
        _logger = logger;
        _validToken = options.Value.Token;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(AUTH_HEADER, out var authHeader) ||
            !authHeader.ToString().StartsWith(BEARER_PREFIX))
        {
            _logger.LogWarning("Missing or malformed Authorization header.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("{\"error\": \"Unauthorized: Missing or invalid token.\"}");
            return;
        }

        var token = authHeader.ToString()[BEARER_PREFIX.Length..].Trim();

        if (token != _validToken)
        {
            _logger.LogWarning("Invalid token received.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("{\"error\": \"Unauthorized: Invalid token.\"}");
            return;
        }

        await _next(context);
    }
}