using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserApi.Services;
using Serilog;
using UserApi.Options;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .WriteTo.File("Logs/audit-log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom services
builder.Services.AddSingleton<IUserService, UserService>();

builder.Services.Configure<AuthenticationOptions>(
    builder.Configuration.GetSection("Authentication"));

builder.Services.Configure<RateLimitingOptions>(
    builder.Configuration.GetSection("RateLimiting"));


var app = builder.Build();

app.UseMiddleware<UserApi.Middleware.ExceptionHandlingMiddleware>();         // 🔴 First: catch all exceptions

app.UseMiddleware<UserApi.Middleware.RateLimitingMiddleware>();              // ⏱️ Second: block abusive traffic early

app.UseMiddleware<UserApi.Middleware.AuthenticationMiddleware>();            // 🔐 Third: validate token after rate check

app.UseMiddleware<UserApi.Middleware.RequestResponseLoggingMiddleware>();   // 📝 Fourth: log requests and responses

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();