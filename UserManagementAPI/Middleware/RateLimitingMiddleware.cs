using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using UserApi.Options;

namespace UserApi.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RequestCounter> _requestCounts = new();
    private readonly int _limit;
    private readonly TimeSpan _window;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IOptions<RateLimitingOptions> options)
    {
        _next = next;
        _logger = logger;
        _limit = options.Value.Limit;
        _window = TimeSpan.FromMinutes(options.Value.WindowMinutes);
    }

public async Task Invoke(HttpContext context)
{
    var key = GetClientKey(context);
    var now = DateTime.UtcNow;

    var counter = _requestCounts.GetOrAdd(key, _ => new RequestCounter(now));
    bool isRateLimited = false;

    lock (counter)
    {
        if (now - counter.WindowStart >= _window)
        {
            counter.Count = 0;
            counter.WindowStart = now;
        }

        counter.Count++;

        if (counter.Count > _limit)
        {
            isRateLimited = true;
        }
    }

    if (isRateLimited)
    {
        _logger.LogWarning("Rate limit exceeded for {key}", key);
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.Response.WriteAsync("{\"error\": \"Rate limit exceeded. Try again later.\"}");
        return;
    }

    await _next(context);
}

    private string GetClientKey(HttpContext context)
    {
        // Use IP address or token for identification
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private class RequestCounter
    {
        public int Count;
        public DateTime WindowStart;

        public RequestCounter(DateTime start)
        {
            Count = 0;
            WindowStart = start;
        }
    }
}