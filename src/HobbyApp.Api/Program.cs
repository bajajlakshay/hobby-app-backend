using System.Threading.RateLimiting;
using HobbyApp.Api.Services;
using HobbyApp.Application;
using HobbyApp.Application.Common.Interfaces;
using HobbyApp.Infrastructure;
using HobbyApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Clean Architecture layers.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Exposes the authenticated user's claims to the application layer.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// The API is consumed from browsers when the app runs as a web build. Auth is
// bearer-token based (no cookies), so a permissive CORS policy is safe here.
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Per-IP rate limits on the auth endpoints (behind Caddy the client IP comes
// from X-Forwarded-For, applied by UseForwardedHeaders before this runs).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            """{"errors":["Too many requests. Please wait a moment and try again."]}""",
            cancellationToken);
    };

    // General credential endpoints: login, register, verify, refresh.
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
            }));

    // Endpoints that send email — much stricter to prevent inbox flooding.
    options.AddPolicy("otp", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(5),
            }));
});

var app = builder.Build();

// Apply any pending EF Core migrations on startup so the database schema is
// always current (in production no EF tooling is needed on the server).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    // In production the API runs behind Caddy, which terminates TLS and forwards
    // requests over plain HTTP on the internal Docker network. Honor the proxy's
    // X-Forwarded-* headers so the app sees the original https scheme/host.
    // Caddy already redirects HTTP -> HTTPS at the edge, so no in-app redirect.
    var forwardedOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    };
    // Trust the proxy regardless of its container IP on the internal network
    // (only Caddy can reach the API; it is not published to the host).
    forwardedOptions.KnownIPNetworks.Clear();
    forwardedOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedOptions);
}

app.UseCors();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
