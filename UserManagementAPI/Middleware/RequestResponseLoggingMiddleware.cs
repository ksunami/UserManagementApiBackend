using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace UserApi.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;

        context.Request.EnableBuffering();
        var requestBody = await ReadStreamAsync(context.Request.Body);
        context.Request.Body.Position = 0;

        var maskedRequest = MaskSensitiveData(requestBody);

        _logger.LogInformation("Request [{correlationId}]: {method} {path} | Query: {query} | Headers: {headers} | Body: {body}",
            correlationId,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            JsonSerializer.Serialize(context.Request.Headers),
            maskedRequest);

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        _logger.LogInformation("Response [{correlationId}]: {statusCode} | Body: {body}",
            correlationId,
            context.Response.StatusCode,
            responseText);

        await responseBody.CopyToAsync(originalBodyStream);
    }

    private async Task<string> ReadStreamAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private string MaskSensitiveData(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var dict = doc.RootElement.EnumerateObject()
                .ToDictionary(
                    prop => prop.Name,
                    prop => prop.Name.ToLower().Contains("password") ? "***MASKED***" : prop.Value.ToString()
                );
            return JsonSerializer.Serialize(dict);
        }
        catch
        {
            return json; // fallback if not valid JSON
        }
    }
}